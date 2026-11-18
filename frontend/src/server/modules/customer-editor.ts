import 'server-only';
import { z } from 'zod';
import { one, rows, transaction, update, type Executor, type Row } from '../db';
import { requirePermission, requireStaff, type Session } from '../auth/session';
import { assert } from '../errors';
import { customers, scopeCustomer } from './customers';
import { users } from './users';
import { randomUUID } from 'node:crypto';

const uuid=z.string().uuid();
const companySchema=z.object({LongName:z.string().trim().min(1).max(2000),ShortName:z.string().trim().min(1).max(1000),INNNumber:z.string().max(64).default(''),KPPNumber:z.string().max(64).default(''),PhoneNumber:z.string().max(256).default(''),Email:z.union([z.literal(''),z.string().email()]).default(''),SiteUrl:z.string().max(2000).default(''),RegionId:uuid.nullable().default(null),Address:z.string().max(2000).default(''),CommentNotes:z.string().max(10000).default('')}).strict();
const metaSchema=z.object({organizationType:z.enum(['administration','institution','education','commercial']).default('administration'),networkEdition:z.string().max(5000).default(''),subordinateIds:z.array(uuid).max(100).default([]),bitrixUrl:z.union([z.literal(''),z.string().url().refine(value=>['http:','https:'].includes(new URL(value).protocol))]).default('')}).strict();
const contactSchema=z.object({fullName:z.string().trim().min(1).max(384),jobPost:z.string().max(1000).default(''),phone:z.string().max(16).default(''),email:z.string().email().max(256),birthday:z.string().date().nullable().default(null),userName:z.string().trim().min(1).max(256),password:z.string().max(1024).default('')}).strict();
const schema=z.object({company:companySchema,meta:metaSchema,contact:contactSchema.nullable().default(null),responsibleManagerId:uuid.nullable().default(null)}).strict();
function extras(value:unknown):Row {try{const result=JSON.parse(String(value||'{}'));return result&&typeof result==='object'&&!Array.isArray(result)?result:{};}catch{return {};}}
async function editorData(s:Session,id:string,db:Executor){
  const customer=await scopeCustomer(s,id,db);
  const contact=customer.ContactUserProfileId?await one(`SELECT p."IdentityUserId",p."LastName",p."FirstName",p."MiddleName",p."JobPost",p."Email",p."DateOfBirthDay",u."UserName",u."PhoneNumber" FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."Id"=$1 AND p."TenantId"=$2 AND NOT u."IsDeleted"`,[customer.ContactUserProfileId,id],db):null;
  return {company:Object.fromEntries(Object.keys(companySchema.shape).map(key=>[key,customer[key]??(key==='RegionId'?null:'')])),responsibleManagerId:customer.ResponsibleManagerId,meta:metaSchema.parse(extras(customer.ExtraProperties).NextCustomerUi??{}),contact:contact?{fullName:[contact.LastName,contact.FirstName,contact.MiddleName].filter(Boolean).join(' '),jobPost:contact.JobPost??'',email:contact.Email??'',phone:contact.PhoneNumber??'',birthday:contact.DateOfBirthDay?new Date(String(contact.DateOfBirthDay)).toISOString().slice(0,10):null,userName:contact.UserName,password:''}:null};
}
export async function customerEditor(s:Session,id:string|undefined,method:string,body:unknown){
  requireStaff(s);
  if(method==='GET'){requirePermission(s,'CustomerManagement.Get');assert(id,404,'Клиент не найден');return transaction(db=>editorData(s,id,db));}
  requirePermission(s,'CustomerManagement.'+(id?'Edit':'Create'));
  const input=schema.parse(body);
  return transaction(async db=>{
    if(input.responsibleManagerId)assert(await one(`SELECT e."Id" FROM "EmployeeProfiles" e JOIN "AbpUsers" u ON u."Id"=e."IdentityUserId" WHERE e."Id"=$1 AND NOT u."IsDeleted" AND u."IsActive"`,[input.responsibleManagerId],db),400,'Ответственный не найден');
    if(input.meta.subordinateIds.length){const found=await rows(`SELECT "Id" FROM "TenantProfiles" WHERE "Id"=ANY($1::uuid[]) AND NOT "IsDeleted"`,[input.meta.subordinateIds],db);assert(found.length===new Set(input.meta.subordinateIds).size&&!input.meta.subordinateIds.includes(id??''),400,'Проверьте подведомственные организации');}
    const current=id?await scopeCustomer(s,id,db,true):await customers(s,undefined,undefined,'POST',input.company,new URLSearchParams()) as Row;
    const customerId=String(current.Id);let contactId:unknown=current.ContactUserProfileId??null;
    if(input.contact){
      const c=input.contact,[lastName='',firstName='',...middle]=c.fullName.split(/\s+/),birthday=c.birthday?c.birthday+'T12:00:00Z':null;
      const existing=contactId?await one(`SELECT "IdentityUserId" FROM "CustomerUserProfiles" WHERE "Id"=$1 AND "TenantId"=$2`,[contactId,customerId],db):null;
      if(existing){
        const identityId=String(existing.IdentityUserId);
        await users(s,identityId,undefined,'PATCH',{firstName,lastName,middleName:middle.join(' '),email:c.email,phone:c.phone||null,jobPost:c.jobPost,birthday},new URLSearchParams());
        const identity=await one(`SELECT "UserName" FROM "AbpUsers" WHERE "Id"=$1`,[identityId],db);
        if(identity!.UserName!==c.userName){
          await db.query('SELECT pg_advisory_xact_lock(hashtextextended($1,0))',[`identity:${customerId}:${c.userName.toUpperCase()}`]);
          assert(!await one(`SELECT "Id" FROM "AbpUsers" WHERE "TenantId"=$1 AND "NormalizedUserName"=$2 AND "Id"<>$3 AND NOT "IsDeleted"`,[customerId,c.userName.toUpperCase(),identityId],db),409,'Логин уже используется');
          await update('AbpUsers',identityId,{UserName:c.userName,NormalizedUserName:c.userName.toUpperCase(),SecurityStamp:randomUUID()},db,s.userId);await db.query('DELETE FROM "NextSessions" WHERE "UserId"=$1',[identityId]);
        }
        if(c.password)await users(s,identityId,'password','POST',{password:c.password},new URLSearchParams());
      }else{
        assert(c.password.length>=8,400,'Для нового контактного лица укажите пароль от 8 символов');
        const created=await users(s,undefined,undefined,'POST',{userName:c.userName,password:c.password,email:c.email,firstName,lastName,middleName:middle.join(' '),phone:c.phone||null,jobPost:c.jobPost,birthday,tenantId:customerId},new URLSearchParams()) as {id:string};
        contactId=(await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1 AND "TenantId"=$2`,[created.id,customerId],db))!.Id;
      }
    }else contactId=null;
    await update('TenantProfiles',customerId,{...input.company,ResponsibleManagerId:input.responsibleManagerId,ContactUserProfileId:contactId,ExtraProperties:JSON.stringify({...extras(current.ExtraProperties),NextCustomerUi:input.meta})},db,s.userId);
    return {id:customerId};
  });
}
