import 'server-only';
import { z } from 'zod';
import { one, rows, transaction, insert, pool } from '../db';
import { requireStaff, requirePermission, scopeTicket, type Session } from '../auth/session';
import { scopeContract } from './contracts';
import { assert } from '../errors';
export async function tags(s:Session,kind:'tickets'|'contracts',id:string,method:string,body:unknown) {
  const scope=()=>kind==='tickets'?scopeTicket(s,id,pool()):scopeContract(s,id);
  await scope();requireStaff(s);requirePermission(s,kind==='tickets'?'Ticket.Edit':'Contract.Edit');
  const table=kind==='tickets'?'TicketTags':'ContractTags',column=kind==='tickets'?'TicketId':'ContractId';
  if(method==='GET')return rows(`SELECT t.* FROM "Tags" t JOIN "${table}" l ON l."TagId"=t."Id" WHERE l."${column}"=$1`,[id]);
  const data=z.object({tagId:z.string().uuid().optional(),name:z.string().trim().min(1).max(1000).optional()}).strict().parse(body);assert(data.tagId||data.name,400,'Укажите тег');
  return transaction(async db=>{
    if(kind==='tickets')await scopeTicket(s,id,db,true);else await scopeContract(s,id,db,true);
    let tag=data.tagId?await one(`SELECT * FROM "Tags" WHERE "Id"=$1`,[data.tagId],db):await one(`SELECT * FROM "Tags" WHERE "Name"=$1`,[data.name],db);
    if(!tag && data.name && method!=='DELETE')tag=await insert('Tags',{Name:data.name},db,s.userId);
    assert(tag,404,'Тег не найден');
    if(method==='DELETE')await db.query(`DELETE FROM "${table}" WHERE "${column}"=$1 AND "TagId"=$2`,[id,tag.Id]);
    else await db.query(`INSERT INTO "${table}"("${column}","TagId") VALUES($1,$2) ON CONFLICT DO NOTHING`,[id,tag.Id]);
    return {ok:true};
  });
}
