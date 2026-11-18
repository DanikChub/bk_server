import 'server-only';
import { z } from 'zod';
import { one, rows, insert, transaction, pool } from '../db';
import { scopeTicket, requirePermission, type Session } from '../auth/session';
import { assert } from '../errors';
export async function messages(s:Session,ticketId:string,method:string,body:unknown,query:URLSearchParams) {
  await scopeTicket(s,ticketId,pool());requirePermission(s,'TicketMessage.'+(method==='GET'?'Get':'Create'));
  if(method==='GET') {
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
    return {items:await rows(`SELECT m.*,p."FirstName",p."LastName" FROM "TicketMessages" m LEFT JOIN "CustomerUserProfiles" p ON p."Id"=m."CreatorCustomerUserProfileId" WHERE m."TicketId"=$1 ORDER BY m."CreationTime" DESC LIMIT $2 OFFSET $3`,[ticketId,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count FROM "TicketMessages" WHERE "TicketId"=$1`,[ticketId]))!.count};
  }
  const data=z.object({comment:z.string().trim().min(1).max(1_000_000)}).strict().parse(body);
  return transaction(async db=> {
    const ticket=await scopeTicket(s,ticketId,db,true);
    await one(`SELECT "Id" FROM "AbpUsers" WHERE "Id"=$1 FOR UPDATE`,[s.userId],db);
    let profile=await one(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1`,[s.userId],db);
    if(!profile&&!s.tenantId){
      const employee=await one(`SELECT * FROM "EmployeeProfiles" WHERE "IdentityUserId"=$1`,[s.userId],db);
      if(employee)profile=await insert('CustomerUserProfiles',{Id:s.userId,IdentityUserId:s.userId,TenantId:null,FirstName:employee.FirstName,LastName:employee.LastName,MiddleName:employee.MiddleName,Email:employee.Email,JobPost:employee.JobPost},db,s.userId);
    }
    assert(profile,409,'Профиль не найден');
    const message=await insert('TicketMessages',{Comment:data.comment,TicketId:ticketId,CreatorCustomerUserProfileId:profile.Id},db,s.userId);
    if(ticket.CreatorId){
      const recipient=await one(`SELECT "TenantId" FROM "AbpUsers" WHERE "Id"=$1 AND NOT "IsDeleted"`,[ticket.CreatorId],db),category=await one(`SELECT "Id" FROM "NotificationCategories" WHERE lower("Name") LIKE '%заявк%' ORDER BY "Id" LIMIT 1`,[],db);
      if(recipient)await insert('UserNotifications',{IdentityUserId:ticket.CreatorId,Title:`Сообщение по заявке ${ticketId}`,Url:recipient.TenantId?`/client/ticket?id=${ticketId}`:`/tickets/${ticketId}#messages`,NotificationCategoryId:category?.Id??null},db,s.userId);
    }
    return message;
  });
}
