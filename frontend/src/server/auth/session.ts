import 'server-only';
import { createHash, randomBytes } from 'node:crypto';
import { cookies } from 'next/headers';
import { one, rows, type Executor, type Row, pool, transaction } from '../db';
import { assert, AppError } from '../errors';
import { verifyPassword } from './password';
import { verifyAuthenticatorCode } from './totp';
import { rotatePassword } from './change-password';

export const sessionCookie = 'consulting_session';
export type Session = { userId: string; tenantId: string | null; userName: string; roles: string[]; permissions: string[]; specialistId: string | null };
const tokenHash = (token: string) => createHash('sha256').update(token).digest('hex');
export async function loadSession(db: Executor = pool()): Promise<Session> {
  const token = (await cookies()).get(sessionCookie)?.value;
  assert(token && /^[a-f0-9]{64}$/.test(token),401,'Требуется вход');
  const user = await one(`SELECT u."Id",u."TenantId",u."UserName",u."TwoFactorEnabled" FROM "NextSessions" s JOIN "AbpUsers" u ON u."Id"=s."UserId"
    WHERE s."TokenHash"=$1 AND s."ExpiresAt">now() AND NOT u."IsDeleted" AND u."IsActive"
    AND s."TenantId" IS NOT DISTINCT FROM u."TenantId" AND s."SecurityStamp" IS NOT DISTINCT FROM u."SecurityStamp"
    AND (NOT u."LockoutEnabled" OR u."LockoutEnd" IS NULL OR u."LockoutEnd"<=now())
    AND (u."TenantId" IS NULL OR EXISTS(SELECT 1 FROM "AbpTenants" t LEFT JOIN "TenantProfiles" p ON p."Id"=t."Id" WHERE t."Id"=u."TenantId" AND NOT t."IsDeleted" AND (p."Id" IS NULL OR NOT p."IsDeleted")))`,[tokenHash(token)],db);
  assert(user,401,'Сессия истекла');
  const roles = await rows<{Name:string}>(`SELECT r."Name" FROM "AbpRoles" r JOIN "AbpUserRoles" ur ON ur."RoleId"=r."Id" WHERE ur."UserId"=$1 AND r."TenantId" IS NOT DISTINCT FROM $2::uuid`,[user.Id,user.TenantId],db);
  const roleNames = roles.map(r=>r.Name);
  const grants = await rows<{Name:string}>(`SELECT DISTINCT "Name" FROM "AbpPermissionGrants" WHERE "TenantId" IS NOT DISTINCT FROM $1::uuid
    AND (("ProviderName"='U' AND "ProviderKey"=$2) OR ("ProviderName"='R' AND "ProviderKey"=ANY($3::text[])))`,[user.TenantId,user.Id,roleNames],db);
  const specialist = await one(`SELECT "Id" FROM "EmployeeProfiles" WHERE "IdentityUserId"=$1`,[user.Id],db);
  return { userId: String(user.Id), tenantId: user.TenantId ? String(user.TenantId) : null, userName: String(user.UserName), roles: roleNames, permissions: grants.map(g=>g.Name), specialistId: specialist ? String(specialist.Id) : null };
}
export function isHostAdmin(s: Session) { return s.tenantId === null && s.roles.includes('admin'); }
export function requirePermission(s: Session, ...permissions: string[]) {
  assert(isHostAdmin(s) || permissions.every(p=>s.permissions.includes(p)),403,'Недостаточно прав');
}
export function requireStaff(s: Session) { assert(s.tenantId === null && !s.roles.includes('Клиент'),403,'Доступно сотрудникам'); }
export async function login(userName: string, password: string, tenantName: string | null, clientKey: string, authenticatorCode?:string,newPassword?:string): Promise<string> {
  return transaction(async db => {
    const key = tokenHash(`${clientKey}:${tenantName?.toUpperCase() ?? 'host'}:${userName.toUpperCase()}`);
    const attempt = await one(`INSERT INTO "NextLoginAttempts"("Key","WindowStart","Count") VALUES($1,now(),1)
      ON CONFLICT("Key") DO UPDATE SET "Count"=CASE WHEN "NextLoginAttempts"."WindowStart"<now()-interval '15 minutes' THEN 1 ELSE "NextLoginAttempts"."Count"+1 END,
      "WindowStart"=CASE WHEN "NextLoginAttempts"."WindowStart"<now()-interval '15 minutes' THEN now() ELSE "NextLoginAttempts"."WindowStart" END RETURNING "Count"`,[key],db);
    if (Number(attempt!.Count)>20) return { error: new AppError(429,'Слишком много попыток входа') };
    const tenants=tenantName?await rows(`SELECT t."Id" FROM "AbpTenants" t LEFT JOIN "TenantProfiles" p ON p."Id"=t."Id" WHERE NOT t."IsDeleted" AND (p."Id" IS NULL OR NOT p."IsDeleted") AND (lower(t."Name")=lower($1) OR t."Id"::text=$1 OR lower(p."ShortName")=lower($1))`,[tenantName],db):[];
    const tenant=tenants.length===1?tenants[0]:null;
    const user = tenantName && !tenant ? undefined : await one(`SELECT * FROM "AbpUsers" WHERE "NormalizedUserName"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid AND NOT "IsDeleted" FOR UPDATE`,[userName.toUpperCase(),tenant?.Id ?? null],db);
    // A dummy derivation keeps unknown users on the same password verification path.
    const valid = await verifyPassword(password, user?.PasswordHash as string ?? 'AQAAAAIAAYagAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==');
    if (!user || !valid || !user.IsActive || user.LockoutEnabled && user.LockoutEnd && new Date(String(user.LockoutEnd))>new Date()) {
      if (user && !valid) await db.query(`UPDATE "AbpUsers" SET "AccessFailedCount"="AccessFailedCount"+1,
        "LockoutEnd"=CASE WHEN "LockoutEnabled" AND "AccessFailedCount">=4 THEN now()+interval '5 minutes' ELSE "LockoutEnd" END WHERE "Id"=$1`,[user.Id]);
      return { error: new AppError(401,'Неверные данные входа') };
    }
    if(user.TwoFactorEnabled){
      const key=await one(`SELECT "Value" FROM "AbpUserTokens" WHERE "UserId"=$1 AND "LoginProvider"='[AspNetUserStore]' AND "Name"='AuthenticatorKey'`,[user.Id],db);
      if(!key?.Value)return {error:new AppError(409,'Для аккаунта требуется дополнительный способ подтверждения входа','mfa_provider_required')};
      if(!authenticatorCode || !verifyAuthenticatorCode(String(key.Value),authenticatorCode)){
        if(authenticatorCode)await db.query(`UPDATE "AbpUsers" SET "AccessFailedCount"="AccessFailedCount"+1,"LockoutEnd"=CASE WHEN "LockoutEnabled" AND "AccessFailedCount">=4 THEN now()+interval '5 minutes' ELSE "LockoutEnd" END WHERE "Id"=$1`,[user.Id]);
        return {error:new AppError(401,'Введите действующий код из приложения аутентификации','mfa_required')};
      }
    }
    if(user.ShouldChangePasswordOnNextLogin){
      if(!newPassword)return {error:new AppError(409,'Необходимо изменить пароль перед входом','password_change_required')};
      assert(newPassword.length>=8 && newPassword.length<=1024,400,'Новый пароль должен содержать не менее 8 символов');
      user.SecurityStamp=await rotatePassword(db,String(user.Id),newPassword,String(user.Id));
    }
    await db.query(`UPDATE "AbpUsers" SET "AccessFailedCount"=0 WHERE "Id"=$1`,[user.Id]);
    await db.query(`DELETE FROM "NextLoginAttempts" WHERE "Key"=$1`,[key]);
    const token = randomBytes(32).toString('hex');
    await db.query(`INSERT INTO "NextSessions"("TokenHash","UserId","TenantId","SecurityStamp","ExpiresAt") VALUES($1,$2,$3,$4,now()+interval '12 hours')`,[tokenHash(token),user.Id,user.TenantId,user.SecurityStamp]);
    return { token };
  }).then(result=>{ if (result.error) throw result.error; return result.token!; });
}
export async function logout() {
  const jar = await cookies(); const token = jar.get(sessionCookie)?.value;
  if (token) await rows(`DELETE FROM "NextSessions" WHERE "TokenHash"=$1`,[tokenHash(token)]);
  jar.delete(sessionCookie);
}
export async function scopeTicket(s: Session, id: string, db: Executor, lock = false): Promise<Row> {
  assert(/^\d+$/.test(id),400,'Некорректный номер заявки');
  const ticket = await one(`SELECT * FROM "Tickets" WHERE "Id"=$1 AND NOT "IsDeleted" ${s.tenantId ? 'AND "TenantId"=$2' : ''} ${lock ? 'FOR UPDATE' : ''}`,s.tenantId ? [id,s.tenantId] : [id],db);
  assert(ticket,404,'Заявка не найдена');
  if (s.tenantId) {
    const status = await one(`SELECT "IsPublic","Name" FROM "TicketStatuses" WHERE "Id"=$1`,[ticket.TicketStatusId],db);
    assert((status?.IsPublic && status?.Name!=='Draft') || ticket.CreatorId === s.userId,404,'Заявка не найдена');
  } else { requireStaff(s); requirePermission(s,'Ticket','Ticket.Get'); }
  return ticket;
}
