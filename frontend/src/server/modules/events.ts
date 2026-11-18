import 'server-only';
import { z } from 'zod';
import { insert, one, rows, transaction, update, pool } from '../db';
import { requirePermission, scopeTicket, type Session } from '../auth/session';
import { assert } from '../errors';
const input=z.object({title:z.string().min(1).max(500),description:z.string().max(10000).nullable().default(null),date:z.string().datetime({offset:true}),type:z.number().int().min(0).max(1).default(0),ticketId:z.string().regex(/^\d+$/).nullable().default(null)}).strict();
export async function events(s:Session,id:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  if(method==='GET') {
    requirePermission(s,'Event.Get');
    if(id){z.string().uuid().parse(id);const event=await one(`SELECT * FROM "Events" WHERE "Id"=$1 AND ("TenantId" IS NOT DISTINCT FROM $2::uuid OR "CreatorId"=$3) AND NOT "IsDeleted"`,[id,s.tenantId,s.userId]);assert(event,404,'Событие не найдено');return event;}
    if(query.get('ticketId')) {await scopeTicket(s,query.get('ticketId')!,pool());return rows(`SELECT * FROM "Events" WHERE "TicketId"=$1 AND NOT "IsDeleted" ORDER BY "EventDate"`,[query.get('ticketId')]);}
    const start=z.string().datetime({offset:true}).parse(query.get('start')),end=z.string().datetime({offset:true}).parse(query.get('end'));
    assert(Date.parse(end)>Date.parse(start),400,'Проверьте период');
    if(query.get('mine')==='true')return rows(`SELECT * FROM "Events" WHERE NOT "IsDeleted" AND "CreatorId"=$1 AND "EventDate">$2 AND "EventDate"<$3 ORDER BY "EventDate"`,[s.userId,start,end]);
    return rows(`SELECT * FROM "Events" WHERE NOT "IsDeleted" AND "TenantId" IS NOT DISTINCT FROM $1::uuid AND "EventDate">$2 AND "EventDate"<$3 ORDER BY "EventDate"`,[s.tenantId,start,end]);
  }
  return transaction(async db=> {
    if(!id) {
      const data=input.parse(body);
      const ticket=data.ticketId?await scopeTicket(s,data.ticketId,db):null;
      if(!ticket)requirePermission(s,'Calendar.Create');
      return insert('Events',{Title:data.title,Description:data.description,EventDate:data.date,EventType:data.type,TicketId:data.ticketId,TenantId:ticket?.TenantId??s.tenantId},db,s.userId);
    }
    z.string().uuid().parse(id);requirePermission(s,method==='DELETE'?'Calendar.Delete':'Calendar.Edit');
    const event=await one(`SELECT * FROM "Events" WHERE "Id"=$1 AND NOT "IsDeleted" AND ("TenantId" IS NOT DISTINCT FROM $2::uuid OR "CreatorId"=$3) FOR UPDATE`,[id,s.tenantId,s.userId],db);assert(event,404,'Событие не найдено');
    if(method==='DELETE')return update('Events',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);
    const data=input.partial().parse(body);
    if(data.ticketId)await scopeTicket(s,data.ticketId,db);
    const mapping:Record<string,string>={title:'Title',description:'Description',date:'EventDate',type:'EventType',ticketId:'TicketId'};
    return update('Events',id,Object.fromEntries(Object.entries(data).map(([k,v])=>[mapping[k],v])),db,s.userId);
  });
}
