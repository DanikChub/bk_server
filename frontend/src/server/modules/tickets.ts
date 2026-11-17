import 'server-only';
import { z } from 'zod';
import { insert, one, rows, transaction, update, pool, type Executor, type Row } from '../db';
import { assert } from '../errors';
import { requirePermission, requireStaff, scopeTicket, type Session } from '../auth/session';
import { enqueue } from '../jobs/queue';
import { productionCalendar } from '../integrations/calendar';
import { legacyDeadline } from './deadline';

const uuid = z.string().uuid();
const createInput = z.object({subject:z.string().trim().min(1).max(10000),description:z.string().max(1_000_000),ticketTypeId:uuid,ticketSectionId:uuid.nullable().default(null),contractId:uuid.nullable().default(null),tenantId:uuid.nullable().default(null),draft:z.boolean().default(false)}).strict();
const historyTypes = { customer:0, specialist:1, responsible:2, notPickUp:3, limit:4, internal:5, status:6 };
export async function addHistory(db: Executor, s: Session, ticket: Row, description: string, type: number, internal=false) {
  return insert('TicketHistory',{TicketId:ticket.Id,TicketStatusId:ticket.TicketStatusId,Description:description,Type:type,IsInternal:internal},db,s.userId);
}
async function status(db: Executor, name: string) {
  const value = await one(`SELECT * FROM "TicketStatuses" WHERE lower("Name")=lower($1)`,[name],db);
  assert(value,409,'Справочник статусов не инициализирован'); return value;
}
async function changeStatus(db: Executor,s: Session,ticket: Row,statusId: string) {
  if(ticket.TicketStatusId === statusId) return ticket;
  const next = await one(`SELECT * FROM "TicketStatuses" WHERE "Id"=$1`,[statusId],db);
  assert(next,400,'Неизвестный статус');
  ticket = (await update('Tickets',ticket.Id,{TicketStatusId:statusId},db,s.userId))!;
  await addHistory(db,s,ticket,`Статус заявки изменён на "${next.DisplayName}"`,historyTypes.status);
  return ticket;
}
export async function listTickets(s: Session, query: URLSearchParams) {
  if(!s.tenantId) { requireStaff(s); requirePermission(s,'Ticket','Ticket.Get'); }
  const args: unknown[] = [];
  const param = (v:unknown) => {args.push(v);return '$'+args.length;};
  const conditions = ['NOT t."IsDeleted"'];
  if(s.tenantId) {
    conditions.push(`t."TenantId"=${param(s.tenantId)}`);
    conditions.push(`((st."IsPublic" AND st."Name"<>'Draft') OR t."CreatorId"=${param(s.userId)})`);
  }
  const search=query.get('search');
  if(search) { const p=param('%'+search+'%'); conditions.push(`(t."Subject" ILIKE ${p} OR t."Id"::text ILIKE ${p} OR tp."ShortName" ILIKE ${p})`); }
  for(const [key,column] of [['statusId','TicketStatusId'],['typeId','TicketTypeId'],['sectionId','TicketSectionId'],['tenantId','TenantId'],['responsibleId','ResponsibleId'],['contractId','ContractId']] as const) {
    const value=query.get(key); if(value) conditions.push(`t."${column}"=${param(uuid.parse(value))}`);
  }
  for(const [key,column] of [['subject','t."Subject"'],['customer','tp."LongName"'],['creator','concat_ws(\' \',u."Surname",u."Name")'],['specialist','concat_ws(\' \',ep."LastName",ep."FirstName",ep."MiddleName")']] as const){const value=query.get(key);if(value)conditions.push(`${column} ILIKE ${param('%'+value.trim()+'%')}`);}
  for(const [key,column,operator] of [['createdAfter','CreationTime','>='],['createdBefore','CreationTime','<='],['dueAfter','DueDate','>='],['dueBefore','DueDate','<=']] as const){const value=query.get(key);if(value)conditions.push(`t."${column}"${operator}${param(z.coerce.date().parse(value))}`);}
  if(query.get('tagId'))conditions.push(`EXISTS(SELECT 1 FROM "TicketTags" tag WHERE tag."TicketId"=t."Id" AND tag."TagId"=${param(uuid.parse(query.get('tagId')))})`);
  if(query.get('packageId'))conditions.push(`EXISTS(SELECT 1 FROM "ContractServicePackages" p WHERE p."ContractId"=t."ContractId" AND p."ServicePackageId"=${param(uuid.parse(query.get('packageId')))})`);
  if(query.get('unassigned')==='true')conditions.push('t."ResponsibleId" IS NULL');
  if(query.get('mine')==='true') conditions.push(`t."ResponsibleId"=${param(s.specialistId)}`);
  if(query.get('status')) conditions.push(`lower(st."Name")=lower(${param(query.get('status'))})`);
  if(query.get('favorite')==='true') {
    const table = s.tenantId ? 'TicketClientFavorites' : 'TicketSpecialistFavorites';
    const column = s.tenantId ? 'ClientId' : 'SpecialistId';
    const profile = s.tenantId ? await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1`,[s.userId]) : null;
    conditions.push(`EXISTS(SELECT 1 FROM "${table}" f WHERE f."TicketId"=t."Id" AND f."${column}"=${param(s.tenantId ? profile?.Id : s.specialistId)})`);
  }
  const skip=z.coerce.number().int().min(0).default(0).parse(query.get('skip')??undefined);
  const take=z.coerce.number().int().min(1).max(200).default(30).parse(query.get('take')??undefined);
  const sorts: Record<string,string> = {created:'t."CreationTime"',id:'t."Id"',subject:'t."Subject"',deadline:'t."DueDate"',status:'st."DisplayName"',customer:'tp."ShortName"',specialist:'ep."LastName"',creator:'u."Surname"',type:'tt."Name"',section:'se."Name"'};
  const sort=sorts[query.get('sort')??'created']; assert(sort,400,'Неизвестная сортировка');
  const direction=query.get('direction')==='asc'?'ASC':'DESC';
  const from=`FROM "Tickets" t JOIN "TicketStatuses" st ON st."Id"=t."TicketStatusId" LEFT JOIN "TenantProfiles" tp ON tp."Id"=t."TenantId" LEFT JOIN "EmployeeProfiles" ep ON ep."Id"=t."ResponsibleId" LEFT JOIN "AbpUsers" u ON u."Id"=t."CreatorId" LEFT JOIN "TicketTypes" tt ON tt."Id"=t."TicketTypeId" LEFT JOIN "TicketSections" se ON se."Id"=t."TicketSectionId" WHERE ${conditions.join(' AND ')}`;
  return transaction(async db=> {
    const count=await one(`SELECT count(*)::integer AS count ${from}`,args,db);
    const items=await rows(`SELECT t.*,st."Name" AS "StatusName",st."DisplayName" AS "StatusDisplayName",tp."ShortName" AS "CustomerName",ep."LastName" AS "SpecialistLastName",ep."FirstName" AS "SpecialistFirstName",tt."Name" AS "TypeName",se."Name" AS "SectionName" ${from} ORDER BY ${sort} ${direction},t."Id" DESC LIMIT $${args.length+1} OFFSET $${args.length+2}`,[...args,take,skip],db);
    return {items,totalCount:count!.count};
  });
}
export async function ticketDetails(s:Session,id:string):Promise<Row & {history:Row[];attachments:Row[];historyAttachments:Row[]}> {
  const ticket=await scopeTicket(s,id,pool());
  if(!s.tenantId)requirePermission(s,'TicketHistory.Get');
  const history=await rows(`SELECT h.*,u."Name" AS "CreatorName",u."Surname" AS "CreatorSurname" FROM "TicketHistory" h LEFT JOIN "AbpUsers" u ON u."Id"=h."CreatorId" WHERE h."TicketId"=$1 AND NOT h."IsDeleted" ${s.tenantId?'AND NOT h."IsInternal" AND h."Type"<>5':''} ORDER BY h."CreationTime",h."Id"`,[id]);
  const attachments=await rows(`SELECT f.* FROM "UploadedFiles" f JOIN "TicketAttachments" a ON a."UploadedFileId"=f."Id" WHERE a."TicketId"=$1`,[id]);
  const historyAttachments=await rows(`SELECT a."TicketHistoryId",f.* FROM "TicketHistoryAttachments" a JOIN "UploadedFiles" f ON f."Id"=a."UploadedFileId" JOIN "TicketHistory" h ON h."Id"=a."TicketHistoryId" WHERE h."TicketId"=$1 AND NOT h."IsDeleted" ${s.tenantId?'AND NOT h."IsInternal" AND h."Type"<>5':''}`,[id]);
  const status=await one(`SELECT * FROM "TicketStatuses" WHERE "Id"=$1`,[ticket.TicketStatusId]);
  const type=await one(`SELECT * FROM "TicketTypes" WHERE "Id"=$1`,[ticket.TicketTypeId]);
  const section=ticket.TicketSectionId?await one(`SELECT * FROM "TicketSections" WHERE "Id"=$1`,[ticket.TicketSectionId]):null;
  const creator=await one(`SELECT "Name","Surname","Email","PhoneNumber" FROM "AbpUsers" WHERE "Id"=$1`,[ticket.CreatorId]);
  const customer=ticket.TenantId?await one(`SELECT "ShortName","LongName" FROM "TenantProfiles" WHERE "Id"=$1 AND NOT "IsDeleted"`,[ticket.TenantId]):null;
  const specialist=ticket.ResponsibleId?await one(`SELECT "FirstName","LastName","MiddleName","JobPost" FROM "EmployeeProfiles" WHERE "Id"=$1`,[ticket.ResponsibleId]):null;
  const limits=ticket.ContractId?await rows(`SELECT c."Id",c."Name",coalesce(m."Sum",0) AS used,coalesce(m."MaxCount",s."Max",0) AS max FROM "ConstraintTypes" c LEFT JOIN "ContractSettings" s ON s."ConstraintTypeId"=c."Id" AND s."ContractId"=$1 LEFT JOIN "ContractConstraintsByMonths" m ON m."ConstraintTypeId"=c."Id" AND m."ContractId"=$1 AND m."Year"=extract(year FROM now()) AND m."Month"=extract(month FROM now())`,[ticket.ContractId]):[];
  return {...ticket,history,attachments,historyAttachments,status,type,section,creator,customer,specialist,limits};
}
export async function createTicket(s:Session,body:unknown,options:{allowExpiredContract?:boolean}={}) {
  const input=createInput.parse(body);
  if(!s.tenantId) {requireStaff(s);requirePermission(s,'Ticket.Create');}
  return transaction(async db=> {
    const tenantId=s.tenantId??input.tenantId;
    let contract:Row|undefined;
    if(input.contractId) contract=await one(`SELECT * FROM "Contracts" WHERE "Id"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid AND NOT "IsDeleted"`,[input.contractId,tenantId],db);
    else if(tenantId) contract=await one(`SELECT * FROM "Contracts" WHERE "TenantId"=$1 AND NOT "IsDeleted" ORDER BY "ContractFinishDate" DESC LIMIT 1`,[tenantId],db);
    assert(contract || (!tenantId && s.tenantId===null),409,'Нет действующего договора');
    if(contract && s.tenantId && !options.allowExpiredContract) assert(new Date(String(contract.ContractFinishDate))>=new Date(),409,'Нет действующего договора');
    const type=await one(`SELECT *,extract(epoch FROM "SLATime") AS "SLASeconds" FROM "TicketTypes" WHERE "Id"=$1`,[input.ticketTypeId],db);
    assert(type,400,'Неизвестный тип заявки');
    if(input.ticketSectionId) assert(await one(`SELECT "Id" FROM "TicketSections" WHERE "Id"=$1`,[input.ticketSectionId],db),400,'Неизвестный раздел');
    const st=await status(db,input.draft?'Draft':'New');
    return insert('Tickets',{Subject:input.subject,Description:input.description,TicketTypeId:input.ticketTypeId,TicketSectionId:input.ticketSectionId,TicketStatusId:st.Id,TenantId:tenantId,ContractId:contract?.Id??null,ResponsibleId:null,Rating:0,AttachmentsCount:0,DueDate:new Date(Date.now()+Number(type.SLASeconds)*1000)},db,s.userId);
  });
}
export async function mutateTicket(s:Session,id:string,action:string,body:unknown) {
  return transaction(async db=> {
    let ticket=await scopeTicket(s,id,db,true);
    if(action==='reply') {
      const input=z.object({description:z.string().max(1_000_000),statusId:uuid.optional(),internal:z.boolean().default(false),templateId:uuid.optional(),constraintTypeId:uuid.optional(),spent:z.number().int().min(0).max(1_000_000).default(0)}).strict().parse(body);
      if(!s.tenantId) requirePermission(s,'TicketHistory.Create');
      else assert(!input.internal && !input.templateId && !input.constraintTypeId && !input.spent && !input.statusId,403,'Недоступные поля ответа');
      const template=input.templateId ? await one(`SELECT "Description" FROM "AnswerTemplates" WHERE "Id"=$1`,[input.templateId],db) : undefined;
      if(input.templateId) assert(template,400,'Шаблон не найден');
      const description=[template?.Description,input.description].filter(Boolean).join(' ');
      assert(description.trim(),400,'Введите сообщение');
      if(s.tenantId) {
        const current=await one(`SELECT "Name" FROM "TicketStatuses" WHERE "Id"=$1`,[ticket.TicketStatusId],db);
        if(current?.Name==='Answered')ticket=await changeStatus(db,s,ticket,String((await status(db,'Renewed')).Id));
      }
      const history=await addHistory(db,s,ticket,description,input.internal?historyTypes.internal:s.tenantId?historyTypes.customer:historyTypes.specialist,input.internal);
      if(input.statusId) ticket=await changeStatus(db,s,ticket,input.statusId);
      if(input.constraintTypeId) {
        assert(ticket.ContractId,409,'Нет действующего договора');
        await insert('TicketHoursSpentHistories',{ConstraintTypeId:input.constraintTypeId,TicketHistoryId:history.Id,Spent:input.spent},db,s.userId);
      }
      return history;
    }
    if(action==='favorite') {
      const {favorite}=z.object({favorite:z.boolean()}).strict().parse(body);
      const profile=s.tenantId?await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1 AND "TenantId"=$2`,[s.userId,s.tenantId],db):null;
      const owner=s.tenantId?profile?.Id:s.specialistId; assert(owner,409,'Профиль пользователя не найден');
      const table=s.tenantId?'TicketClientFavorites':'TicketSpecialistFavorites', column=s.tenantId?'ClientId':'SpecialistId';
      if(favorite) await db.query(`INSERT INTO "${table}"("TicketId","${column}") VALUES($1,$2) ON CONFLICT DO NOTHING`,[id,owner]);
      else await db.query(`DELETE FROM "${table}" WHERE "TicketId"=$1 AND "${column}"=$2`,[id,owner]);
      return {favorite};
    }
    if(action==='read') return update('Tickets',id,{[s.tenantId?'ReadByClientDate':'ReadBySpecialistDate']:new Date()},db,s.userId);
    if(action==='rating') {
      assert(s.tenantId,403,'Оценка доступна клиенту');
      const {rating}=z.object({rating:z.number().min(0).max(5)}).strict().parse(body);
      return update('Tickets',id,{Rating:rating},db,s.userId);
    }
    if(action==='draft') {
      const input=createInput.omit({tenantId:true}).extend({retainedAttachmentIds:z.array(uuid).max(100).optional()}).parse(body);
      const st=await status(db,'Draft'); assert(ticket.CreatorId===s.userId && ticket.TicketStatusId===st.Id,403,'Можно изменять только собственный черновик');
      // Reuse contract/type validation; preserve the existing ticket rather than inserting a second one.
      const type=await one(`SELECT extract(epoch FROM "SLATime") AS seconds FROM "TicketTypes" WHERE "Id"=$1`,[input.ticketTypeId],db); assert(type,400,'Неизвестный тип');
      const contractId=input.contractId??ticket.ContractId;
      if(contractId) assert(await one(`SELECT "Id" FROM "Contracts" WHERE "Id"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid AND NOT "IsDeleted" AND "ContractFinishDate">=now()`,[contractId,ticket.TenantId],db),409,'Нет действующего договора');
      else assert(!s.tenantId,409,'Нет действующего договора');
      if(input.retainedAttachmentIds) {
        const existing=await rows(`SELECT "UploadedFileId" FROM "TicketAttachments" WHERE "TicketId"=$1`,[id],db);
        assert(input.retainedAttachmentIds.every(fileId=>existing.some(f=>f.UploadedFileId===fileId)),400,'Недопустимое вложение');
        const removed=existing.filter(f=>!input.retainedAttachmentIds!.includes(String(f.UploadedFileId)));
        await db.query(`DELETE FROM "TicketAttachments" WHERE "TicketId"=$1 AND NOT ("UploadedFileId"=ANY($2::uuid[]))`,[id,input.retainedAttachmentIds]);
        for(const file of removed)await enqueue(db,'file.delete-if-unreferenced',{id:file.UploadedFileId,tenantId:ticket.TenantId});
        await update('Tickets',id,{AttachmentsCount:input.retainedAttachmentIds.length},db,s.userId);
      }
      ticket=(await update('Tickets',id,{Subject:input.subject,Description:input.description,TicketTypeId:input.ticketTypeId,TicketSectionId:input.ticketSectionId,ContractId:contractId,DueDate:new Date(Date.now()+Number(type.seconds)*1000)},db,s.userId))!;
      if(!input.draft) ticket=await changeStatus(db,s,ticket,String((await status(db,'New')).Id));
      return ticket;
    }
    requireStaff(s);
    if(action==='assign') {
      const {specialistId}=z.object({specialistId:uuid.nullable()}).strict().parse(body);
      requirePermission(s,specialistId?'SpecialistManagement.Create':'SpecialistManagement.Delete');
      if(ticket.ResponsibleId===specialistId) return ticket;
      const employee=specialistId?await one(`SELECT e.* FROM "EmployeeProfiles" e JOIN "AbpUsers" u ON u."Id"=e."IdentityUserId" WHERE e."Id"=$1 AND u."IsActive" AND NOT u."IsDeleted"`,[specialistId],db):null;
      if(specialistId) assert(employee,400,'Специалист не найден');
      await addHistory(db,s,ticket,employee?`Заявка назначена на сотрудника ${employee.LastName??''}`:'Заявка снята со специалиста',historyTypes.responsible);
      return update('Tickets',id,{ResponsibleId:specialistId},db,s.userId);
    }
    requirePermission(s,'Ticket.Edit');
    if(action==='status') return changeStatus(db,s,ticket,z.object({statusId:uuid}).strict().parse(body).statusId);
    if(action==='description') return update('Tickets',id,{Description:z.object({description:z.string().max(1_000_000)}).strict().parse(body).description},db,s.userId);
    if(action==='section') return update('Tickets',id,{TicketSectionId:z.object({sectionId:uuid.nullable()}).strict().parse(body).sectionId},db,s.userId);
    if(action==='type') {
      const {typeId}=z.object({typeId:uuid}).strict().parse(body);
      const type=await one(`SELECT extract(epoch FROM "SLATime") AS seconds FROM "TicketTypes" WHERE "Id"=$1`,[typeId],db); assert(type,400,'Неизвестный тип');
      const now=new Date(),calendar=await productionCalendar(now.toISOString(),new Date(now.getTime()+30*86400_000).toISOString());
      return update('Tickets',id,{TicketTypeId:typeId,DueDate:legacyDeadline(now,Number(type.seconds),calendar.holidays,process.env.BUSINESS_TIMEZONE??'Europe/Moscow')},db,s.userId);
    }
    if(action==='constraint') {
      const input=z.object({constraintTypeId:uuid,value:z.number().int().min(0).max(1_000_000)}).strict().parse(body);
      assert(ticket.ContractId,409,'Нет действующего договора');
      const constraint=await one(`SELECT "Name" FROM "ConstraintTypes" WHERE "Id"=$1`,[input.constraintTypeId],db); assert(constraint,400,'Неизвестный лимит');
      const history=await addHistory(db,s,ticket,'',historyTypes.specialist);
      if(input.value) await addHistory(db,s,ticket,`Установлено ограничение типа "${constraint.Name}" в размере ${input.value}`,historyTypes.limit);
      return insert('TicketHoursSpentHistories',{Spent:input.value,ConstraintTypeId:input.constraintTypeId,TicketHistoryId:history.Id},db,s.userId);
    }
    if(action==='not-pick-up') {
      const employee=await one(`SELECT "Phone" FROM "EmployeeProfiles" WHERE "Id"=$1`,[ticket.ResponsibleId],db); assert(employee,409,'Нет исполнителя заявки');
      return addHistory(db,s,ticket,`Специалист не смог вам дозвониться с номера ${employee.Phone??''}`,historyTypes.notPickUp);
    }
    assert(false,404,'Операция не найдена');
  });
}
export async function deleteTicket(s:Session,id:string) {
  requireStaff(s); requirePermission(s,'Ticket.Delete');
  assert(s.roles.some(r=>r==='admin'||r==='Руководитель отдела продаж'),403,'Удаление недоступно');
  return transaction(async db=>{await scopeTicket(s,id,db,true);return update('Tickets',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);});
}
export async function editHistory(s:Session,id:string,historyId:string,method:string,body:unknown) {
  uuid.parse(historyId); requireStaff(s); requirePermission(s,method==='DELETE'?'TicketHistory.Delete':'TicketHistory.Edit');
  if(method==='DELETE') requirePermission(s,'Ticket.Delete');
  return transaction(async db=> {
    await scopeTicket(s,id,db,true);
    assert(await one(`SELECT "Id" FROM "TicketHistory" WHERE "Id"=$1 AND "TicketId"=$2 AND NOT "IsDeleted"`,[historyId,id],db),404,'Запись не найдена');
    return update('TicketHistory',historyId,method==='DELETE'?{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId}:{Description:z.object({description:z.string().max(1_000_000)}).strict().parse(body).description},db,s.userId);
  });
}
