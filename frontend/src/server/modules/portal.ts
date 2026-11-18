import 'server-only';
import { rows, one, transaction, update } from '../db';
import type { Row } from '../db';
import type { Session } from '../auth/session';
import { requirePermission } from '../auth/session';
import { assert } from '../errors';
import type { PortalSnapshot, PortalAttachment, PortalTicket } from '@/entities/client-portal/model/types';
import { z } from 'zod';
import { createTicket } from './tickets';
import { scopeContract } from './contracts';

const text=(v:unknown)=>String(v??'');
const iso=(v:unknown)=>v?new Date(v as string).toISOString():'';
const fullName=(r:Row)=>[r.LastName,r.FirstName,r.MiddleName].filter(Boolean).join(' ');
const attachment=(r:Row):PortalAttachment=>({id:text(r.Id),name:text(r.FileName),size:Number(r.SizeInBytes),type:text(r.ContentType)});
export async function portalSnapshot(s:Session):Promise<PortalSnapshot> {
  assert(s.tenantId,403,'Кабинет доступен пользователю организации');
  const profile=await one(`SELECT p.*,u."PhoneNumber" FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."IdentityUserId"=$1 AND p."TenantId"=$2`,[s.userId,s.tenantId]);assert(profile,404,'Профиль не найден');
  const organization=await one(`SELECT * FROM "TenantProfiles" WHERE "Id"=$1 AND NOT "IsDeleted"`,[s.tenantId]);assert(organization,404,'Организация не найдена');
  const tickets=await rows(`SELECT t.*,st."Name" AS status,se."Name" AS section,e."FirstName",e."LastName",e."MiddleName",e."JobPost" AS "SpecialistJob",e."Phone" AS "SpecialistPhone",tt."Name" AS "TypeName",concat_ws(' ',author."Surname",author."Name") AS "AuthorName",EXISTS(SELECT 1 FROM "TicketClientFavorites" f WHERE f."TicketId"=t."Id" AND f."ClientId"=$2) AS favorite FROM "Tickets" t JOIN "TicketStatuses" st ON st."Id"=t."TicketStatusId" LEFT JOIN "TicketSections" se ON se."Id"=t."TicketSectionId" LEFT JOIN "EmployeeProfiles" e ON e."Id"=t."ResponsibleId" LEFT JOIN "TicketTypes" tt ON tt."Id"=t."TicketTypeId" LEFT JOIN "AbpUsers" author ON author."Id"=t."CreatorId" WHERE t."TenantId"=$1 AND NOT t."IsDeleted" AND ((st."IsPublic" AND st."Name"<>'Draft') OR t."CreatorId"=$3) ORDER BY t."CreationTime" DESC`,[s.tenantId,profile.Id,s.userId]);
  const ids=tickets.map(t=>t.Id);
  const history=await rows(`SELECT h.*,u."Name" AS "FirstName",u."Surname" AS "LastName" FROM "TicketHistory" h LEFT JOIN "AbpUsers" u ON u."Id"=h."CreatorId" WHERE h."TicketId"=ANY($1::bigint[]) AND NOT h."IsDeleted" AND NOT h."IsInternal" AND h."Type"<>5 ORDER BY h."CreationTime",h."Id"`,[ids]);
  const files=await rows(`SELECT a."TicketId",f.* FROM "TicketAttachments" a JOIN "UploadedFiles" f ON f."Id"=a."UploadedFileId" WHERE a."TicketId"=ANY($1::bigint[])`,[ids]);
  const historyFiles=await rows(`SELECT a."TicketHistoryId",f.* FROM "TicketHistoryAttachments" a JOIN "UploadedFiles" f ON f."Id"=a."UploadedFileId" WHERE a."TicketHistoryId"=ANY($1::uuid[])`,[history.map(h=>h.Id)]);
  const allContracts=await rows(`SELECT c.* FROM "Contracts" c WHERE c."TenantId"=$1 AND NOT c."IsDeleted" ORDER BY c."ContractFinishDate" DESC`,[s.tenantId]);
  const packages=await rows(`SELECT p."Name",l.* FROM "ContractServicePackages" l JOIN "ServicePackages" p ON p."Id"=l."ServicePackageId" JOIN "Contracts" c ON c."Id"=l."ContractId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted"`,[s.tenantId]);
  const limits=await rows(`SELECT m.*,t."Name" FROM "ContractConstraintsByMonths" m JOIN "ConstraintTypes" t ON t."Id"=m."ConstraintTypeId" JOIN "Contracts" c ON c."Id"=m."ContractId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted" ORDER BY m."Year" DESC,m."Month" DESC`,[s.tenantId]);
  const colleagues=await rows(`SELECT p."Id",p."FirstName",p."LastName",p."MiddleName" FROM "CustomerUserProfiles" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."TenantId"=$1 AND u."IsActive" AND NOT u."IsDeleted" ORDER BY p."LastName"`,[s.tenantId]);
  const events=await rows(`SELECT * FROM "Events" WHERE "TenantId"=$1 AND "CreatorId"=$2 AND NOT "IsDeleted" ORDER BY "EventDate"`,[s.tenantId,s.userId]);
  const statuses:Record<string,PortalTicket['status']>={New:'Новая',InProgress:'В работе',MoreDetails:'Требует уточнения',Draft:'Черновик',Answered:'Закрыта',Renewed:'Возобновленная',WaitingForSpecialistResponse:'Ожидание ответа'};
  return {
    profile:{id:text(profile.Id),name:fullName(profile),position:text(profile.JobPost),phone:text(profile.PhoneNumber),email:text(profile.Email),birthDate:iso(profile.DateOfBirthDay),avatarUrl:profile.UserAvatarFileId?'/api/v1/profile/avatar':undefined,organization:{name:text(organization.LongName),address:text(organization.Address),inn:text(organization.INNNumber),kpp:text(organization.KPPNumber),active:Boolean(organization.IsActive)},colleagues:colleagues.map(r=>({id:text(r.Id),name:fullName(r)}))},
    tickets:tickets.map(t=>({id:text(t.Id),subject:text(t.Subject),description:text(t.Description),status:statuses[text(t.status)]??'В работе',section:text(t.section),createdAt:iso(t.CreationTime),updatedAt:iso(t.LastModificationTime??t.CreationTime),dueDate:iso(t.DueDate),type:text(t.TypeName),authorName:text(t.AuthorName),specialistJob:text(t.SpecialistJob),specialistPhone:text(t.SpecialistPhone),packages:packages.filter(p=>p.ContractId===t.ContractId).map(p=>text(p.Name)),specialist:fullName(t)||'Не назначен',favorite:Boolean(t.favorite),rating:Number(t.Rating)||null,attachments:files.filter(f=>f.TicketId===t.Id).map(attachment),messages:history.filter(h=>h.TicketId===t.Id).map(h=>({id:text(h.Id),kind:([2,3,4,6].includes(Number(h.Type))?'event':Number(h.Type)===0?'client':'reply') as 'event'|'client'|'reply',creatorId:text(h.CreatorId),read:Boolean(h.ReadBySpecialistDate),author:fullName(h)||'Система',text:text(h.Description),createdAt:iso(h.CreationTime),attachments:historyFiles.filter(f=>f.TicketHistoryId===h.Id).map(attachment)}))})),
    contracts:allContracts.map(c=>{
      const contractLimits=limits.filter(l=>l.ContractId===c.Id),months=[...new Set(contractLimits.map(l=>`${l.Year}-${String(l.Month).padStart(2,'0')}`))];
      const start=+new Date(text(c.ContractStartDate)),end=+new Date(text(c.ContractFinishDate));
      return {id:text(c.Id),number:text(c.Name),package:packages.filter(p=>p.ContractId===c.Id).map(p=>p.Name).join(', '),startsAt:iso(c.ContractStartDate),endsAt:iso(c.ContractFinishDate),active:end>=Date.now(),progress:end>start?Math.max(0,Math.min(100,Math.round((Date.now()-start)/(end-start)*100))):100,months:months.map(month=>({month,limits:contractLimits.filter(l=>`${l.Year}-${String(l.Month).padStart(2,'0')}`===month).map(l=>({title:text(l.Name),total:Number(l.MaxCount),used:Number(l.Sum)}))}))};
    }),
    events:events.map(e=>{let extra:Record<string,unknown>={};try{extra=JSON.parse(text(e.ExtraProperties)||'{}');}catch{}return {id:text(e.Id),title:text(e.Title),date:iso(e.EventDate),completed:Boolean(extra.completed),important:e.EventType===1};}),
    content:{news:[],articles:[]},
  };
}
export async function portalProfile(s:Session,body:unknown) {
  assert(s.tenantId,403,'Кабинет клиента недоступен');
  const data=z.object({position:z.string().max(1000),phone:z.string().max(16),email:z.string().email().max(256),birthDate:z.string().refine(v=>!v||Number.isFinite(Date.parse(v)),'Некорректная дата')}).strict().parse(body);
  await transaction(async db=>{
    const profile=await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1 AND "TenantId"=$2`,[s.userId,s.tenantId],db);assert(profile,404,'Профиль не найден');
    await update('CustomerUserProfiles',profile.Id,{JobPost:data.position,Email:data.email,DateOfBirthDay:data.birthDate||null},db,s.userId);
    await update('AbpUsers',s.userId,{Email:data.email,NormalizedEmail:data.email.toUpperCase(),PhoneNumber:data.phone},db,s.userId);
  });
  return (await portalSnapshot(s)).profile;
}
export async function completeEvent(s:Session,id:string,body:unknown) {
  const {completed}=z.object({completed:z.boolean()}).strict().parse(body);z.string().uuid().parse(id);requirePermission(s,'Calendar.Edit');
  await transaction(async db=>{
    const event=await one(`SELECT * FROM "Events" WHERE "Id"=$1 AND "CreatorId"=$2 AND "TenantId" IS NOT DISTINCT FROM $3::uuid AND NOT "IsDeleted" FOR UPDATE`,[id,s.userId,s.tenantId],db);assert(event,404,'Событие не найдено');
    const extra=JSON.parse(text(event.ExtraProperties)||'{}');extra.completed=completed;
    await update('Events',id,{ExtraProperties:JSON.stringify(extra)},db,s.userId);
  });return (await portalSnapshot(s)).events.find(e=>e.id===id);
}
export async function renewal(s:Session,body:unknown) {
  assert(s.tenantId,403,'Кабинет клиента недоступен');requirePermission(s,'Ticket.Create');
  const data=z.object({contractId:z.string().uuid(),description:z.string().trim().min(1).max(1_000_000)}).strict().parse(body);
  const contract=await scopeContract(s,data.contractId);
  const type=await one(`SELECT "Id" FROM "TicketTypes" WHERE "Name"='Письменная'`);assert(type,409,'Тип письменной заявки не настроен');
  return createTicket(s,{subject:`Продление договора ${contract.Name}`,description:data.description,ticketTypeId:type.Id,contractId:data.contractId},{allowExpiredContract:true});
}
