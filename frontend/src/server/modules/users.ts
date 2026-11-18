import 'server-only';
import { randomUUID } from 'node:crypto';
import { z } from 'zod';
import { one, rows, insert, transaction, update, pool, type Executor, type Row } from '../db';
import { assert } from '../errors';
import { isHostAdmin, requirePermission, requireStaff, type Session } from '../auth/session';
import { hashPassword, verifyPassword } from '../auth/password';
import { rotatePassword } from '../auth/change-password';
import { createClientRole } from './customers';
import { encryptLegacyPassword, decryptLegacyPassword } from '../auth/legacy-encryption';

const userFields=`u."Id",u."TenantId",u."UserName",u."Name",u."Surname",u."Email",u."PhoneNumber",u."IsActive",u."CreationTime",u."LockoutEnd"`;
const newUser=z.object({userName:z.string().trim().min(1).max(256),password:z.string().min(8).max(1024),email:z.string().email().max(256),firstName:z.string().max(64).default(''),lastName:z.string().max(64).default(''),middleName:z.string().max(256).default(''),phone:z.string().max(16).nullable().default(null),tenantId:z.string().uuid().nullable().default(null),jobPost:z.string().max(1000).default(''),birthday:z.string().datetime({offset:true}).nullable().default(null),roles:z.array(z.string().max(256)).max(30).default([])}).strict();
export async function createUser(db:Executor,data:z.infer<typeof newUser>,actor?:string,profileKind?:'customer'|'employee') {
  await db.query(`SELECT pg_advisory_xact_lock(hashtextextended($1,0))`,[`identity:${data.tenantId??'host'}:${data.userName.toUpperCase()}`]);
  assert(!await one(`SELECT "Id" FROM "AbpUsers" WHERE "NormalizedUserName"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid AND NOT "IsDeleted"`,[data.userName.toUpperCase(),data.tenantId],db),409,'Пользователь с таким логином уже существует');
  const user=await insert('AbpUsers',{UserName:data.userName,NormalizedUserName:data.userName.toUpperCase(),Email:data.email,NormalizedEmail:data.email.toUpperCase(),PasswordHash:await hashPassword(data.password),SecurityStamp:randomUUID(),TenantId:data.tenantId,Name:data.firstName,Surname:data.lastName,PhoneNumber:data.phone,IsActive:true,EmailConfirmed:false,PhoneNumberConfirmed:false,IsExternal:false,TwoFactorEnabled:false,LockoutEnabled:true,AccessFailedCount:0,ShouldChangePasswordOnNextLogin:false,EntityVersion:0},db,actor);
  const customerProfile=profileKind?profileKind==='customer':Boolean(data.tenantId);
  const profileTable=customerProfile?'CustomerUserProfiles':'EmployeeProfiles';
  const profile=await insert(profileTable,{Id:customerProfile?user.Id:randomUUID(),IdentityUserId:user.Id,FirstName:data.firstName,LastName:data.lastName,MiddleName:data.middleName,Email:data.email,JobPost:data.jobPost,...(customerProfile?{TenantId:data.tenantId,DateOfBirthDay:data.birthday}:{Phone:data.phone,Birthday:data.birthday})},db,actor);
  const encrypted=encryptLegacyPassword(data.password);
  if(encrypted)await update(profileTable,profile.Id,{ExtraProperties:JSON.stringify({EncryptedPassword:encrypted})},db,actor);
  if(data.tenantId) await createClientRole(db,data.tenantId);
  for(const roleName of data.tenantId?['Клиент']:data.roles) {
    const role=await one(`SELECT "Id" FROM "AbpRoles" WHERE "Name"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[roleName,data.tenantId],db);assert(role,400,'Роль не найдена');
    await db.query(`INSERT INTO "AbpUserRoles"("UserId","RoleId","TenantId") VALUES($1,$2,$3) ON CONFLICT DO NOTHING`,[user.Id,role.Id,data.tenantId]);
  }
  return {id:user.Id,profileId:profile.Id};
}
async function targetUser(s:Session,id:string,db:Executor=pool()):Promise<Row> {
  z.string().uuid().parse(id);
  const user=await one(`SELECT * FROM "AbpUsers" WHERE "Id"=$1 AND NOT "IsDeleted"`,[id],db);assert(user,404,'Пользователь не найден');
  if(s.tenantId) assert(user.TenantId===s.tenantId && id===s.userId,404,'Пользователь не найден');return user;
}
export async function users(s:Session,id:string|undefined,action:string|undefined,method:string,body:unknown,query:URLSearchParams,employees=false) {
  requireStaff(s);
  const policy=employees?'EmployeeProfileManagement':action==='profile'?'EmployeeManagement':'UserManagement';
  requirePermission(s,policy+'.'+(method==='GET'?'Get':method==='POST'&&(!id||action==='profile')?'Create':method==='DELETE'?'Delete':'Edit'));
  if(method==='GET') {
    if(id&&action==='profile'){
      await targetUser(s,id);const p=await one(`SELECT * FROM "${employees?'EmployeeProfiles':'CustomerUserProfiles'}" WHERE "IdentityUserId"=$1`,[id]);assert(p,404,'Профиль не найден');delete p.ExtraProperties;return p;
    }
    const tenant=query.get('tenantId');if(tenant)z.string().uuid().parse(tenant);
    const args:unknown[]=[],where=['NOT u."IsDeleted"'];
    const param=(value:unknown)=>{args.push(value);return '$'+args.length;};
    if(id) {z.string().uuid().parse(id);args.push(id);where.push(`u."Id"=$${args.length}`);}
    if(employees) where.push('u."TenantId" IS NULL AND EXISTS(SELECT 1 FROM "EmployeeProfiles" p WHERE p."IdentityUserId"=u."Id")');
    else where.push('EXISTS(SELECT 1 FROM "CustomerUserProfiles" p WHERE p."IdentityUserId"=u."Id")');
    if(tenant) {args.push(tenant);where.push(`u."TenantId"=$${args.length}`);}
    if(query.get('search')) {const v=param('%'+query.get('search')!.trim()+'%');where.push(`(u."UserName" ILIKE ${v} OR u."Name" ILIKE ${v} OR u."Surname" ILIKE ${v} OR p."LastName" ILIKE ${v} OR p."JobPost" ILIKE ${v} OR tp."ShortName" ILIKE ${v})`);}
    for(const [key,column] of [['firstName','FirstName'],['lastName','LastName'],['middleName','MiddleName'],['jobPost','JobPost']] as const){const value=query.get(key);if(value)where.push(`p."${column}" ILIKE ${param('%'+value.trim()+'%')}`);}
    if(query.get('state')==='block')where.push('NOT u."IsActive"');
    if(query.get('state')==='active')where.push('EXISTS(SELECT 1 FROM "Contracts" c WHERE c."TenantId"=u."TenantId" AND NOT c."IsDeleted")');
    if(query.get('state')==='inactive')where.push('NOT EXISTS(SELECT 1 FROM "Contracts" c WHERE c."TenantId"=u."TenantId" AND NOT c."IsDeleted")');
    if(query.get('updatedOn'))where.push(`p."LastModificationTime"::date=${param(z.coerce.date().parse(query.get('updatedOn')))}`);
    if(query.get('managerId'))where.push(`tp."ResponsibleManagerId"=${param(z.string().uuid().parse(query.get('managerId')))}`);
    const from=`FROM "AbpUsers" u LEFT JOIN "${employees?'EmployeeProfiles':'CustomerUserProfiles'}" p ON p."IdentityUserId"=u."Id" LEFT JOIN "TenantProfiles" tp ON tp."Id"=u."TenantId" WHERE ${where.join(' AND ')}`;
    const selected=`${userFields},p."JobPost",p."MiddleName",p."Id" AS "ProfileId",p."LastModificationTime" AS "ActualizationTime",tp."ShortName" AS "CustomerName",coalesce(p."Email",u."Email") AS "Email"`;
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
    if(id) {
      const user=await one(`SELECT ${selected} ${from}`,args);assert(user,404,'Пользователь не найден');
      return {...user,roles:await rows(`SELECT r."Id",r."Name" FROM "AbpUserRoles" ur JOIN "AbpRoles" r ON r."Id"=ur."RoleId" WHERE ur."UserId"=$1`,[id])};
    }
    const sorts:Record<string,string>={name:'u."Surname"',login:'u."UserName"',created:'u."CreationTime"',jobPost:'p."JobPost"',customer:'tp."ShortName"'};const sort=sorts[query.get('sort')??'name'];assert(sort,400,'Неизвестная сортировка');const direction=query.get('direction')==='desc'?'DESC':'ASC';
    return {items:await rows(`SELECT ${selected} ${from} ORDER BY ${sort} ${direction},u."Id" LIMIT $${args.length+1} OFFSET $${args.length+2}`,[...args,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count ${from}`,args))!.count};
  }
  return transaction(async db=> {
    if(id && action==='profile' && method==='POST'){
      requirePermission(s,policy+'.Create');
      const user=await targetUser(s,id,db);if(employees)assert(user.TenantId===null,400,'Сотрудник должен принадлежать головной организации');
      const table=employees?'EmployeeProfiles':'CustomerUserProfiles';
      assert(!await one(`SELECT "Id" FROM "${table}" WHERE "IdentityUserId"=$1`,[id],db),409,'Профиль уже существует');
      return insert(table,{Id:employees?randomUUID():id,IdentityUserId:id,FirstName:user.Name??'',LastName:user.Surname??'',MiddleName:'',Email:user.Email,JobPost:'',...(employees?{Phone:user.PhoneNumber}:{TenantId:user.TenantId})},db,s.userId);
    }
    if(!id) {
      const data=newUser.parse(body);
      if(employees) assert(data.tenantId===null,400,'Сотрудник должен принадлежать головной организации');
      if(data.roles.length) assert(isHostAdmin(s),403,'Назначение ролей доступно администратору');
      if(data.tenantId) assert(await one(`SELECT "Id" FROM "TenantProfiles" WHERE "Id"=$1 AND NOT "IsDeleted"`,[data.tenantId],db),400,'Организация не найдена');
      return createUser(db,data,s.userId,employees?'employee':'customer');
    }
    const user=await targetUser(s,id,db);
    if(employees)assert(user.TenantId===null && await one(`SELECT "Id" FROM "EmployeeProfiles" WHERE "IdentityUserId"=$1`,[id],db),404,'Сотрудник не найден');
    else assert(await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1`,[id],db),404,'Профиль пользователя не найден');
    const targetRoles=await rows<{Name:string}>(`SELECT r."Name" FROM "AbpUserRoles" ur JOIN "AbpRoles" r ON r."Id"=ur."RoleId" WHERE ur."UserId"=$1`,[id],db);
    if(targetRoles.some(r=>r.Name==='admin')) assert(isHostAdmin(s),403,'Изменение администратора запрещено');
    if(action==='password') {
      const data=z.object({password:z.string().min(8).max(1024)}).strict().parse(body);
      await rotatePassword(db,id,data.password,s.userId);return {ok:true};
    }
    if(action==='roles') {
      assert(isHostAdmin(s),403,'Требуется администратор');
      const data=z.object({roles:z.array(z.string().max(256)).max(30)}).strict().parse(body);
      assert(id!==s.userId || data.roles.includes('admin'),409,'Нельзя снять собственную роль администратора');
      const roleIds=[];
      for(const name of data.roles) {const role=await one(`SELECT "Id" FROM "AbpRoles" WHERE "Name"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[name,user.TenantId],db);assert(role,400,'Роль не найдена');roleIds.push(role.Id);}
      await db.query(`DELETE FROM "AbpUserRoles" WHERE "UserId"=$1`,[id]);
      for(const roleId of roleIds) await db.query(`INSERT INTO "AbpUserRoles"("UserId","RoleId","TenantId") VALUES($1,$2,$3)`,[id,roleId,user.TenantId]);
      await db.query(`DELETE FROM "NextSessions" WHERE "UserId"=$1`,[id]);return {ok:true};
    }
    if(action==='active') {
      const {active}=z.object({active:z.boolean()}).strict().parse(body);assert(id!==s.userId || active,409,'Нельзя заблокировать себя');
      if(!active)requirePermission(s,policy+'.Delete');
      await update('AbpUsers',id,{IsActive:active,LockoutEnd:null,AccessFailedCount:0},db,s.userId);
      if(!active) await db.query(`DELETE FROM "NextSessions" WHERE "UserId"=$1`,[id]);return {ok:true};
    }
    if(method==='DELETE') {
      assert(id!==s.userId,409,'Нельзя удалить себя');
      await update('AbpUsers',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId,IsActive:false},db,s.userId);
      await db.query(`DELETE FROM "NextSessions" WHERE "UserId"=$1`,[id]);return {ok:true};
    }
    const data=z.object({email:z.string().email().max(256).optional(),firstName:z.string().max(64).optional(),lastName:z.string().max(64).optional(),phone:z.string().max(16).nullable().optional(),middleName:z.string().max(256).optional(),jobPost:z.string().max(1000).optional(),birthday:z.coerce.date().nullable().optional(),country:z.string().max(256).nullable().optional(),city:z.string().max(256).nullable().optional(),street:z.string().max(512).nullable().optional(),note:z.string().max(10000).nullable().optional()}).strict().parse(body);
    if(employees)assert(data.country===undefined&&data.city===undefined&&data.street===undefined&&data.note===undefined,400,'Недопустимые поля профиля сотрудника');
    const mapping:Record<string,string>={email:'Email',firstName:'Name',lastName:'Surname',phone:'PhoneNumber'};
    const changes=Object.fromEntries(Object.entries(data).filter(([k])=>k in mapping).map(([k,v])=>[mapping[k],v]));if(data.email)changes.NormalizedEmail=data.email.toUpperCase();
    if(Object.keys(changes).length)await update('AbpUsers',id,changes,db,s.userId);
    const table=employees?'EmployeeProfiles':'CustomerUserProfiles',profile=await one(`SELECT "Id" FROM "${table}" WHERE "IdentityUserId"=$1`,[id],db);
    const profileMapping:Record<string,string>={email:'Email',firstName:'FirstName',lastName:'LastName',middleName:'MiddleName',jobPost:'JobPost',birthday:employees?'Birthday':'DateOfBirthDay',...(employees?{phone:'Phone'}:{country:'Country',city:'City',street:'Street',note:'Note'})};
    const profileChanges=Object.fromEntries(Object.entries(data).filter(([k])=>k in profileMapping).map(([k,v])=>[profileMapping[k],v]));
    if(profile && Object.keys(profileChanges).length)await update(table,profile.Id,profileChanges,db,s.userId);
    return {ok:true};
  });
}
export async function revealPassword(s:Session,id:string,employee=false) {
  requireStaff(s);requirePermission(s,'EmployeeProfileManagement.Create');
  return transaction(async db=>{
    await targetUser(s,id,db);const table=employee?'EmployeeProfiles':'CustomerUserProfiles';
    const admin=await one(`SELECT 1 FROM "AbpUserRoles" ur JOIN "AbpRoles" r ON r."Id"=ur."RoleId" WHERE ur."UserId"=$1 AND r."Name"='admin' AND r."TenantId" IS NULL`,[id],db);
    if(admin)assert(isHostAdmin(s),403,'Изменение администратора запрещено');
    const p=await one(`SELECT "ExtraProperties" FROM "${table}" WHERE "IdentityUserId"=$1`,[id],db);
    const encrypted=p?JSON.parse(String(p.ExtraProperties||'{}')).EncryptedPassword:null;
    if(!encrypted)return {password:null};
    assert(process.env.LEGACY_PASSWORD_ENCRYPTION_KEY && process.env.LEGACY_ENCRYPTION_SALT_BASE64 && process.env.LEGACY_ENCRYPTION_IV_BASE64,503,'Совместимость со старым шифрованием не настроена');
    return {password:decryptLegacyPassword(encrypted)};
  });
}
export async function profile(s:Session,method:string,body:unknown) {
  if(method==='GET') {
    const user=await one(`SELECT ${userFields} FROM "AbpUsers" u WHERE u."Id"=$1`,[s.userId]);
    const profile=await one(`SELECT * FROM "${s.tenantId?'CustomerUserProfiles':'EmployeeProfiles'}" WHERE "IdentityUserId"=$1`,[s.userId]);
    if(profile) delete profile.ExtraProperties;
    return {user,profile};
  }
  const data=z.object({email:z.string().email().max(256),phone:z.string().max(16).nullable(),firstName:z.string().max(64),lastName:z.string().max(64),middleName:z.string().max(256),jobPost:z.string().max(1000)}).strict().parse(body);
  return transaction(async db=> {
    await update('AbpUsers',s.userId,{Email:data.email,NormalizedEmail:data.email.toUpperCase(),PhoneNumber:data.phone,Name:data.firstName,Surname:data.lastName},db,s.userId);
    const table=s.tenantId?'CustomerUserProfiles':'EmployeeProfiles';const p=await one(`SELECT "Id" FROM "${table}" WHERE "IdentityUserId"=$1`,[s.userId],db);assert(p,404,'Профиль не найден');
    const result=await update(table,p.Id,{Email:data.email,FirstName:data.firstName,LastName:data.lastName,MiddleName:data.middleName,JobPost:data.jobPost,...(!s.tenantId?{Phone:data.phone}:{})},db,s.userId);
    if(result)delete result.ExtraProperties;return result;
  });
}
export async function birthdays(s:Session,query:URLSearchParams) {
  requireStaff(s);requirePermission(s,'CustomerManagement.Get');
  const start=z.coerce.date().parse(query.get('start')),end=z.coerce.date().parse(query.get('end'));assert(end>=start,400,'Проверьте период');
  return rows(`SELECT p."Id",p."IdentityUserId",p."FirstName",p."LastName",p."MiddleName",p."JobPost",p."Email",p."DateOfBirthDay",t."ShortName" AS "CustomerName"
    FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" LEFT JOIN "TenantProfiles" t ON t."Id"=p."TenantId"
    WHERE p."DateOfBirthDay" IS NOT NULL AND NOT u."IsDeleted" AND make_date(extract(year FROM now())::integer,extract(month FROM p."DateOfBirthDay")::integer,
    least(extract(day FROM p."DateOfBirthDay")::integer,extract(day FROM (make_date(extract(year FROM now())::integer,extract(month FROM p."DateOfBirthDay")::integer,1)+interval '1 month - 1 day'))::integer)) BETWEEN $1::date AND $2::date
    ORDER BY extract(month FROM p."DateOfBirthDay"),extract(day FROM p."DateOfBirthDay"),p."LastName"`,[start,end]);
}
export async function changeOwnPassword(s:Session,body:unknown){
  const data=z.object({currentPassword:z.string().min(1).max(1024),newPassword:z.string().min(8).max(1024)}).strict().parse(body);
  await transaction(async db=>{
    const user=await one(`SELECT "PasswordHash" FROM "AbpUsers" WHERE "Id"=$1 AND "IsActive" AND NOT "IsDeleted" FOR UPDATE`,[s.userId],db);
    assert(user && await verifyPassword(data.currentPassword,user.PasswordHash as string),400,'Текущий пароль указан неверно');
    await rotatePassword(db,s.userId,data.newPassword,s.userId);
  });return {reauthenticate:true};
}
export async function identityUsers(s:Session,query:URLSearchParams){
  requireStaff(s);requirePermission(s,'UserManagement.Get');
  const tenant=query.get('tenantId');if(tenant)z.string().uuid().parse(tenant);
  const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
  const args:unknown[]=[tenant??null],where=['NOT u."IsDeleted"','u."TenantId" IS NOT DISTINCT FROM $1::uuid'];
  if(query.get('search')){args.push('%'+query.get('search')!.trim()+'%');where.push(`(u."UserName" ILIKE $2 OR u."Email" ILIKE $2 OR concat_ws(' ',u."Surname",u."Name") ILIKE $2)`);}
  const from=`FROM "AbpUsers" u WHERE ${where.join(' AND ')}`;
  return {items:await rows(`SELECT ${userFields},EXISTS(SELECT 1 FROM "CustomerUserProfiles" p WHERE p."IdentityUserId"=u."Id") AS "HasCustomerProfile",EXISTS(SELECT 1 FROM "EmployeeProfiles" p WHERE p."IdentityUserId"=u."Id") AS "HasEmployeeProfile" ${from} ORDER BY u."UserName",u."Id" LIMIT $${args.length+1} OFFSET $${args.length+2}`,[...args,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count ${from}`,args))!.count};
}
