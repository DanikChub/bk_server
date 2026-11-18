import 'server-only';
import { rows, one, insert, transaction, update, executor } from '../db';
import type { Session } from '../auth/session';
import { requirePermission, requireStaff, scopeTicket } from '../auth/session';
import { scopeCustomer } from './customers';
import { z } from 'zod';
import { assert } from '../errors';
import { listTickets } from './tickets';
export async function customerStatistics(s:Session,customerId:string) {
  await scopeCustomer(s,customerId);
  const sections=await rows(`SELECT m."Year",m."Month",t."Name",sum(m."Count")::integer AS "Count" FROM "ContractTicketsByTicketSectionByMonths" m JOIN "Contracts" c ON c."Id"=m."ContractId" JOIN "TicketSections" t ON t."Id"=m."TicketSectionId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted" GROUP BY m."Year",m."Month",t."Name"`,[customerId]);
  const types=await rows(`SELECT m."Year",m."Month",t."Name",sum(m."Count")::integer AS "Count" FROM "ContractTicketsByTicketTypeByMonths" m JOIN "Contracts" c ON c."Id"=m."ContractId" JOIN "TicketTypes" t ON t."Id"=m."TicketTypeId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted" GROUP BY m."Year",m."Month",t."Name"`,[customerId]);
  const constraints=await rows(`SELECT m."Year",m."Month",t."Name",m."MaxCount",sum(m."Sum")::integer AS "Sum" FROM "ContractConstraintsByMonths" m JOIN "Contracts" c ON c."Id"=m."ContractId" JOIN "ConstraintTypes" t ON t."Id"=m."ConstraintTypeId" WHERE c."TenantId"=$1 AND NOT c."IsDeleted" GROUP BY m."Year",m."Month",t."Name",m."MaxCount"`,[customerId]);
  const dictionary=await rows(`SELECT "Name" FROM "ConstraintTypes" ORDER BY "Name"`),months=[];
  const now=new Date();
  for(let year=2023;year<=now.getUTCFullYear();year++)for(let month=1;month<=12;month++) {
    if(year===now.getUTCFullYear() && month>now.getUTCMonth()+1)break;
    const inMonth=(r:Record<string,unknown>)=>r.Year===year && r.Month===month;
    const spent=constraints.filter(inMonth);
    months.push({Year:year,Month:month,TicketSectionsByMonths:sections.filter(inMonth),TicketTypesByMonths:types.filter(inMonth),ConstraintsByMonths:[...spent,...dictionary.filter(t=>!spent.some(r=>r.Name===t.Name)).map(t=>({Name:t.Name,MaxCount:0,Sum:0}))]});
  }
  return months.reverse();
}
export async function workload(s:Session,query:URLSearchParams) {
  requireStaff(s);requirePermission(s,'Ticket.Get');
  const start=z.string().datetime({offset:true}).parse(query.get('start')),end=z.string().datetime({offset:true}).parse(query.get('end'));
  assert(Date.parse(end)>=Date.parse(start),400,'Проверьте период');
  return rows(`SELECT e."Id",e."FirstName",e."LastName",e."MiddleName",count(t."Id")::integer AS "CountTicketTake" FROM "EmployeeProfiles" e JOIN "Tickets" t ON t."ResponsibleId"=e."Id" JOIN "TicketStatuses" st ON st."Id"=t."TicketStatusId" WHERE NOT t."IsDeleted" AND st."Name"='InProgress' AND EXISTS(SELECT 1 FROM "Tickets" recent WHERE recent."ResponsibleId"=e."Id" AND recent."TicketStatusId"=st."Id" AND NOT recent."IsDeleted" AND recent."CreationTime">$1 AND recent."CreationTime"<$2) GROUP BY e."Id" ORDER BY "CountTicketTake" DESC`,[start,end]);
}
export async function spentTime(s:Session,ticketId:string,body:unknown) {
  requireStaff(s);requirePermission(s,'TicketHoursSpentHistory.Create');
  const data=z.object({historyId:z.string().uuid(),constraintTypeId:z.string().uuid(),spent:z.number().int().min(0).max(1_000_000)}).strict().parse(body);
  return transaction(async db=>{
    await scopeTicket(s,ticketId,db,true);assert(await one(`SELECT "Id" FROM "TicketHistory" WHERE "Id"=$1 AND "TicketId"=$2 AND NOT "IsDeleted"`,[data.historyId,ticketId],db),404,'Запись истории не найдена');
    const constraint=await one(`SELECT "Name" FROM "ConstraintTypes" WHERE "Id"=$1`,[data.constraintTypeId],db);assert(constraint,400,'Ограничение не найдено');
    return {...await insert('TicketHoursSpentHistories',{TicketHistoryId:data.historyId,ConstraintTypeId:data.constraintTypeId,Spent:data.spent},db,s.userId),ConstraintTypeName:constraint.Name};
  });
}
export async function ticketSpent(s:Session,ticketId:string){
  const ticket=await scopeTicket(s,ticketId,executor());
  if(!s.tenantId)requirePermission(s,'TicketHistory.Get');
  return rows(`SELECT c."Id",c."Name",sum(x."Spent")::integer AS "NowCount",coalesce(setting."Max",10) AS "MaxCount"
    FROM "TicketHoursSpentHistories" x JOIN "TicketHistory" h ON h."Id"=x."TicketHistoryId" JOIN "ConstraintTypes" c ON c."Id"=x."ConstraintTypeId"
    LEFT JOIN LATERAL (SELECT s."Max" FROM "ContractSettings" s WHERE s."ContractId"=$2 AND s."ConstraintTypeId"=c."Id" ORDER BY s."CreationTime" DESC,s."Id" DESC LIMIT 1) setting ON true
    WHERE h."TicketId"=$1 AND NOT h."IsDeleted" ${s.tenantId?'AND NOT h."IsInternal" AND h."Type"<>5':''}
    GROUP BY c."Id",c."Name",setting."Max" ORDER BY c."Name",c."Id"`,[ticketId,ticket.ContractId]);
}
export async function readHistory(s:Session,ticketId:string,historyId:string) {
  z.string().uuid().parse(historyId);
  return transaction(async db=>{
    await scopeTicket(s,ticketId,db);
    const history=await one(`SELECT * FROM "TicketHistory" WHERE "Id"=$1 AND "TicketId"=$2 AND NOT "IsDeleted"`,[historyId,ticketId],db);assert(history,404,'Запись не найдена');
    if(s.tenantId)assert(!history.IsInternal && history.Type!==5,404,'Запись не найдена');
    return update('TicketHistory',historyId,{[s.tenantId?'ReadByClientDate':'ReadBySpecialistDate']:new Date()},db,s.userId);
  });
}
export async function statusCounts(s:Session,query:URLSearchParams=new URLSearchParams()) {
  if(!s.tenantId){requireStaff(s);requirePermission(s,'Ticket.Get');}
  const statuses=await rows(`SELECT "Id","Name","DisplayNameMany" FROM "TicketStatuses" ORDER BY "Name"`);
  return Promise.all(statuses.map(async status=>{const filter=new URLSearchParams(query);filter.delete('status');filter.set('statusId',String(status.Id));filter.set('skip','0');filter.set('take','1');return {...status,count:(await listTickets(s,filter)).totalCount};}));
}
