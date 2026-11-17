import 'server-only';
import { randomUUID } from 'node:crypto';
import { z } from 'zod';
import { insert, one, rows, transaction, update, type Executor, type Row, pool } from '../db';
import { assert } from '../errors';
import { requirePermission, requireStaff, type Session } from '../auth/session';
import { validateFields } from './resources';
import { enqueue } from '../jobs/queue';

const fields=['LongName','ShortName','DisplayName','Address','Description','INNNumber','KPPNumber','Email','PhoneNumber','SiteUrl','CommentNotes','RegionId'];
const clientGrants=['Contract.Get','Ticket.Edit','Ticket.Create','StoriesManagement.Get','Event.Get','Calendar.Get','Calendar.Create','Calendar.Edit','Calendar.Delete','TicketAttachments.Create','TicketAttachments.Get'];
export async function createClientRole(db:Executor,tenantId:string) {
  let role=await one(`SELECT * FROM "AbpRoles" WHERE "TenantId"=$1 AND "NormalizedName"=$2`,[tenantId,'КЛИЕНТ'],db);
  role??=await insert('AbpRoles',{TenantId:tenantId,Name:'Клиент',NormalizedName:'КЛИЕНТ',IsDefault:true,IsPublic:true,IsStatic:false,EntityVersion:0},db);
  for(const name of clientGrants) await db.query(`INSERT INTO "AbpPermissionGrants"("Id","TenantId","Name","ProviderName","ProviderKey") SELECT $1::uuid,$2::uuid,$3::varchar,'R','Клиент' WHERE NOT EXISTS(SELECT 1 FROM "AbpPermissionGrants" WHERE "TenantId"=$2::uuid AND "Name"=$3::varchar AND "ProviderName"='R' AND "ProviderKey"='Клиент')`,[randomUUID(),tenantId,name]);
  return role;
}
export async function scopeCustomer(s:Session,id:string,db:Executor=pool(),lock=false):Promise<Row> {
  z.string().uuid().parse(id);
  if(s.tenantId) assert(s.tenantId===id,404,'Организация не найдена');
  else {requireStaff(s);requirePermission(s,'CustomerManagement.Get');}
  const customer=await one(`SELECT * FROM "TenantProfiles" WHERE "Id"=$1 AND NOT "IsDeleted" ${lock?'FOR UPDATE':''}`,[id],db);assert(customer,404,'Организация не найдена');return customer;
}
export async function customers(s:Session,id:string|undefined,action:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  if(method==='GET') {
    if(id) {
      const customer=await scopeCustomer(s,id);
      if(action==='statistics') return {constraints:await rows(`SELECT m.* FROM "ContractConstraintsByMonths" m JOIN "Contracts" c ON c."Id"=m."ContractId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted" ORDER BY m."Year" DESC,m."Month" DESC`,[id]),tickets:await rows(`SELECT st."Name",count(*)::integer AS count FROM "Tickets" t JOIN "TicketStatuses" st ON st."Id"=t."TicketStatusId" WHERE t."TenantId"=$1 AND NOT t."IsDeleted" GROUP BY st."Name"`,[id])};
      if(action==='users') return rows(`SELECT p."Id",p."IdentityUserId",p."FirstName",p."LastName",p."MiddleName",p."JobPost",p."Email",p."DateOfBirthDay",u."UserName",u."PhoneNumber",u."IsActive" FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."TenantId"=$1 AND NOT u."IsDeleted" ORDER BY p."LastName"`,[id]);
      if(action==='events') {
        requireStaff(s);
        const start=query.get('start')?z.coerce.date().parse(query.get('start')):new Date('2023-01-01'),end=query.get('end')?z.coerce.date().parse(query.get('end')):new Date(Date.now()+86400_000);
        const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
        const from=`FROM "TicketHistory" h JOIN "Tickets" t ON t."Id"=h."TicketId" LEFT JOIN "AbpUsers" u ON u."Id"=h."CreatorId" WHERE t."TenantId"=$1 AND NOT t."IsDeleted" AND NOT h."IsDeleted" AND h."CreationTime">$2 AND h."CreationTime"<$3`;
        const events=await rows(`SELECT h."Id",h."Description",h."CreationTime",h."Type",t."Id" AS "TicketId",t."Subject",t."ResponsibleId",coalesce(u."Name",'') AS "CreatorFirstName",coalesce(u."Surname",'') AS "CreatorLastName" ${from} ORDER BY h."CreationTime" DESC LIMIT $4 OFFSET $5`,[id,start,end,take,skip]);
        const items=events.map(event=>({...event,Action:event.Type===2?event.ResponsibleId?`Назначает ответственного на заявку № ${event.TicketId}`:`Закрывает заявку № ${event.TicketId}`:event.Type===4?event.Description:event.Type===5?`Отправляет скрытое для клиента сообщение: ${event.Description}`:`Оставляет комментарий к заявке № ${event.TicketId}`,Url:`/tickets/${event.TicketId}`}));
        return {items,totalCount:(await one(`SELECT count(*)::integer AS count ${from}`,[id,start,end]))!.count};
      }
      if(action==='managers')requirePermission(s,'ManagerManagement.Get');
      const managers=(!s.tenantId && (s.roles.includes('admin')||s.permissions.includes('ManagerManagement.Get')))?await rows(`SELECT e."Id",e."FirstName",e."LastName",e."MiddleName",e."JobPost",e."Email" FROM "TenantProfileManagers" m JOIN "CustomerUserProfiles" e ON e."Id"=m."ManagerId" WHERE m."TenantProfileId"=$1`,[id]):[];
      return action==='managers'?managers:{...customer,managers};
    }
    requireStaff(s);requirePermission(s,'CustomerManagement.Get');
    const values:unknown[]=[],where=['NOT p."IsDeleted"'],param=(value:unknown)=>{values.push(value);return '$'+values.length;};
    const search=query.get('search');if(search){const v=param('%'+search.trim()+'%');where.push(`(p."ShortName" ILIKE ${v} OR p."LongName" ILIKE ${v} OR p."INNNumber" ILIKE ${v} OR p."KPPNumber" ILIKE ${v} OR p."Address" ILIKE ${v} OR r."Name" ILIKE ${v})`);}
    for(const [key,column] of [['longName','LongName'],['shortName','ShortName'],['address','Address']] as const){const value=query.get(key);if(value)where.push(`p."${column}" ILIKE ${param('%'+value.trim()+'%')}`);}
    if(query.get('active'))where.push(`p."IsActive"=${param(z.enum(['true','false']).parse(query.get('active'))==='true')}`);
    if(query.get('managerId')){const v=param(z.string().uuid().parse(query.get('managerId')));where.push(`(p."ResponsibleManagerId"=${v} OR EXISTS(SELECT 1 FROM "TenantProfileManagers" m WHERE m."TenantProfileId"=p."Id" AND m."ManagerId"=${v}))`);}
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
    const sorts:Record<string,string>={name:'p."ShortName"',created:'p."CreationTime"',inn:'p."INNNumber"',region:'r."Name"'};const sort=sorts[query.get('sort')??'name'];assert(sort,400,'Неизвестная сортировка');const direction=query.get('direction')==='desc'?'DESC':'ASC';
    const from=`FROM "TenantProfiles" p LEFT JOIN "Regions" r ON r."Id"=p."RegionId" WHERE ${where.join(' AND ')}`;
    return {items:await rows(`SELECT p.*,r."Name" AS "RegionName" ${from} ORDER BY ${sort} ${direction},p."Id" LIMIT $${values.length+1} OFFSET $${values.length+2}`,[...values,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count ${from}`,values))!.count};
  }
  requireStaff(s);requirePermission(s,'CustomerManagement.'+(method==='POST'&&!id?'Create':method==='DELETE'?'Delete':'Edit'));
  return transaction(async db=> {
    if(!id) {
      const input=validateFields('TenantProfiles',fields,body,false);
      const tenant=await insert('AbpTenants',{Name:String(input.ShortName).slice(0,64),EntityVersion:0},db,s.userId);
      const customer=await insert('TenantProfiles',{...input,Id:tenant.Id,IsActive:false},db,s.userId);
      await createClientRole(db,String(tenant.Id));
      for(const [Code,Title] of [['draft','Черновик'],['cancel','Отменён'],['complete','Завершён'],['in progress','В работе']]) await insert('ContractStatuses',{TenantId:tenant.Id,Code,Title},db,s.userId);
      await enqueue(db,'tenant.created',{id:tenant.Id,name:tenant.Name});
      return customer;
    }
    await scopeCustomer(s,id,db,true);
    if(action==='manager') {
      const input=z.object({managerId:z.string().uuid()}).strict().parse(body);requirePermission(s,method==='DELETE'?'ManagerManagement.Delete':'ManagerManagement.Create');
      if(method==='DELETE') await db.query(`DELETE FROM "TenantProfileManagers" WHERE "TenantProfileId"=$1 AND "ManagerId"=$2`,[id,input.managerId]);
      else {assert(await one(`SELECT p."Id" FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."Id"=$1 AND NOT u."IsDeleted"`,[input.managerId],db),400,'Менеджер не найден');await db.query(`INSERT INTO "TenantProfileManagers"("TenantProfileId","ManagerId") VALUES($1,$2) ON CONFLICT DO NOTHING`,[id,input.managerId]);}
      return {ok:true};
    }
    if(action==='contacts') {
      const input=z.object({responsibleManagerId:z.string().uuid().nullable().optional(),contactUserProfileId:z.string().uuid().nullable().optional()}).strict().parse(body);
      if(input.contactUserProfileId) assert(await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "Id"=$1 AND "TenantId"=$2`,[input.contactUserProfileId,id],db),400,'Контакт не найден');
      if(input.responsibleManagerId) assert(await one(`SELECT e."Id" FROM "EmployeeProfiles" e JOIN "AbpUsers" u ON u."Id"=e."IdentityUserId" WHERE e."Id"=$1 AND NOT u."IsDeleted"`,[input.responsibleManagerId],db),400,'Менеджер не найден');
      return update('TenantProfiles',id,Object.fromEntries(Object.entries(input).map(([k,v])=>[k==='responsibleManagerId'?'ResponsibleManagerId':'ContactUserProfileId',v])),db,s.userId);
    }
    if(method==='DELETE') {
      await db.query(`UPDATE "Tickets" SET "IsDeleted"=true,"DeletionTime"=now(),"DeleterId"=$2 WHERE "TenantId"=$1 AND NOT "IsDeleted"`,[id,s.userId]);
      await db.query(`UPDATE "Contracts" SET "IsDeleted"=true,"DeletionTime"=now(),"DeleterId"=$2 WHERE "TenantId"=$1 AND NOT "IsDeleted"`,[id,s.userId]);
      // Clear restrictive references before removing the non-soft-deletable customer profiles.
      await db.query(`UPDATE "TenantProfiles" SET "ContactUserProfileId"=NULL WHERE "ContactUserProfileId" IN(SELECT "Id" FROM "CustomerUserProfiles" WHERE "TenantId"=$1)`,[id]);
      await db.query(`UPDATE "Tickets" SET "CustomerUserProfileId"=NULL WHERE "CustomerUserProfileId" IN(SELECT "Id" FROM "CustomerUserProfiles" WHERE "TenantId"=$1)`,[id]);
      await db.query(`UPDATE "TicketSpecialistFavorites" SET "CustomerUserProfileId"=NULL WHERE "CustomerUserProfileId" IN(SELECT "Id" FROM "CustomerUserProfiles" WHERE "TenantId"=$1)`,[id]);
      await db.query(`DELETE FROM "CustomerUserProfiles" WHERE "TenantId"=$1`,[id]);
      await db.query(`UPDATE "AbpUsers" SET "IsActive"=false,"LastModificationTime"=now(),"LastModifierId"=$2 WHERE "TenantId"=$1`,[id,s.userId]);
      const result=await update('TenantProfiles',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);
      await update('AbpTenants',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);
      await db.query(`DELETE FROM "NextSessions" WHERE "TenantId"=$1`,[id]);
      await enqueue(db,'tenant.deleted',{id});return result;
    }
    return update('TenantProfiles',id,validateFields('TenantProfiles',fields,body,true),db,s.userId);
  });
}
