import 'server-only';
import { createHash, randomBytes, randomInt } from 'node:crypto';
import { z } from 'zod';
import { one, rows, transaction, type Executor, type Row } from '../db';
import { enqueue } from '../jobs/queue';
import { assert } from '../errors';
import { rotatePassword } from './change-password';
import { emailConfigured } from '../integrations/email';
const hash=(value:string)=>createHash('sha256').update(value).digest('hex');
export async function requestPasswordReset(body:unknown){
  const input=z.object({userName:z.string().trim().min(1).max(256),tenant:z.string().max(64).nullable().default(null)}).strict().parse(body);
  assert(emailConfigured(),503,'Восстановление доступа по почте не настроено');
  const origin=process.env.APP_URL;assert(origin&&new URL(origin).protocol==='https:',503,'HTTPS-адрес приложения не настроен');
  return transaction(async db=>{
    const key=hash('recovery:'+input.tenant?.toUpperCase()+':'+input.userName.toUpperCase());
    const attempt=await one(`INSERT INTO "NextLoginAttempts"("Key","WindowStart","Count") VALUES($1,now(),1) ON CONFLICT("Key") DO UPDATE SET "Count"=CASE WHEN "NextLoginAttempts"."WindowStart"<now()-interval '15 minutes' THEN 1 ELSE "NextLoginAttempts"."Count"+1 END,"WindowStart"=CASE WHEN "NextLoginAttempts"."WindowStart"<now()-interval '15 minutes' THEN now() ELSE "NextLoginAttempts"."WindowStart" END RETURNING "Count"`,[key],db);
    const response={ok:true,message:'Если аккаунт найден, письмо с дальнейшими действиями будет отправлено.'};
    if(Number(attempt!.Count)>5)return response;
    let tenantId:string|null=null;
    if(input.tenant){const tenants=await rows(`SELECT t."Id" FROM "AbpTenants" t LEFT JOIN "TenantProfiles" p ON p."Id"=t."Id" WHERE NOT t."IsDeleted" AND (p."Id" IS NULL OR NOT p."IsDeleted") AND (lower(t."Name")=lower($1) OR t."Id"::text=$1 OR lower(p."ShortName")=lower($1))`,[input.tenant],db);if(tenants.length!==1)return response;tenantId=String(tenants[0].Id);}
    const matches=await rows(`SELECT u.*,coalesce(cp."Email",ep."Email",u."Email") AS "RecoveryEmail" FROM "AbpUsers" u LEFT JOIN "CustomerUserProfiles" cp ON cp."IdentityUserId"=u."Id" LEFT JOIN "EmployeeProfiles" ep ON ep."IdentityUserId"=u."Id" WHERE NOT u."IsDeleted" AND u."IsActive" AND NOT u."IsExternal" AND u."TenantId" IS NOT DISTINCT FROM $2::uuid AND (u."NormalizedUserName"=$1 OR u."NormalizedEmail"=$1 OR upper(coalesce(cp."Email",ep."Email"))=$1) FOR UPDATE OF u`,[input.userName.toUpperCase(),tenantId],db);
    if(matches.length!==1||!z.string().email().safeParse(matches[0].RecoveryEmail).success)return response;
    const user=matches[0],token=randomBytes(32).toString('hex');
    await db.query(`DELETE FROM "NextRecoveryTokens" WHERE "UserId"=$1 AND "Purpose"='password-reset'`,[user.Id]);
    await db.query(`INSERT INTO "NextRecoveryTokens"("TokenHash","UserId","Purpose","SecurityStamp","ExpiresAt") VALUES($1,$2,'password-reset',$3,now()+interval '30 minutes')`,[hash(token),user.Id,user.SecurityStamp]);
    const url=new URL('/client/reset-password',origin);url.hash='token='+token;
    await enqueue(db,'email.send',{to:user.RecoveryEmail,subject:'Восстановление доступа — ИПС Консалтинг',text:`Для изменения пароля откройте ссылку в течение 30 минут: ${url.toString()}\nЕсли вы не запрашивали восстановление, проигнорируйте письмо.`});
    return response;
  });
}
export async function resetPassword(body:unknown){
  const input=z.object({token:z.string().regex(/^[a-f0-9]{64}$/),newPassword:z.string().min(8).max(1024)}).strict().parse(body);
  return transaction(async db=>{
    const recovery=await one(`SELECT * FROM "NextRecoveryTokens" WHERE "TokenHash"=$1 AND "Purpose"='password-reset' AND "ExpiresAt">now() FOR UPDATE`,[hash(input.token)],db);assert(recovery,400,'Ссылка недействительна или истекла');
    const user=await one(`SELECT * FROM "AbpUsers" WHERE "Id"=$1 AND "IsActive" AND NOT "IsDeleted" FOR UPDATE`,[recovery.UserId],db);assert(user&&user.SecurityStamp===recovery.SecurityStamp,400,'Ссылка недействительна или истекла');
    await rotatePassword(db,String(user.Id),input.newPassword,String(user.Id));
    await db.query(`DELETE FROM "NextRecoveryTokens" WHERE "UserId"=$1`,[user.Id]);return {ok:true};
  });
}
export async function emailMfa(db:Executor,user:Row,code?:string){
  if(code){const token=await one(`DELETE FROM "NextRecoveryTokens" WHERE "TokenHash"=$1 AND "UserId"=$2 AND "Purpose"='email-mfa' AND "ExpiresAt">now() AND "SecurityStamp" IS NOT DISTINCT FROM $3 RETURNING "UserId"`,[hash(String(user.Id)+':email-mfa:'+code),user.Id,user.SecurityStamp],db);return Boolean(token);}
  assert(emailConfigured()&&user.EmailConfirmed&&z.string().email().safeParse(user.Email).success,409,'Способ подтверждения входа по почте не настроен');
  const otp=String(randomInt(100000,1000000));
  await db.query(`DELETE FROM "NextRecoveryTokens" WHERE "UserId"=$1 AND "Purpose"='email-mfa'`,[user.Id]);
  await db.query(`INSERT INTO "NextRecoveryTokens"("TokenHash","UserId","Purpose","SecurityStamp","ExpiresAt") VALUES($1,$2,'email-mfa',$3,now()+interval '5 minutes')`,[hash(String(user.Id)+':email-mfa:'+otp),user.Id,user.SecurityStamp]);
  await enqueue(db,'email.send',{to:user.Email,subject:'Код входа — ИПС Консалтинг',text:`Ваш код входа: ${otp}. Действителен 5 минут. Никому не сообщайте код.`});return false;
}
