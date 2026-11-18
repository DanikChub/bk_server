import 'server-only';
import { z } from 'zod';
import { one, rows, transaction } from '../db';
import { type Session } from '../auth/session';
import { assert } from '../errors';
export async function notifications(s:Session,id:string|undefined,action:string|undefined,method:string,query:URLSearchParams) {
  if(action==='categories')return rows(`SELECT * FROM "NotificationCategories" ORDER BY "Name"`);
  if(action==='statuses')return rows(`SELECT st.*,count(n."Id")::integer AS "Count" FROM "NotificationStatuses" st LEFT JOIN "UserNotifications" n ON n."IdentityUserId"=$1 AND (st."Status"=2 OR st."Status"=0 AND n."Seen" IS NULL OR st."Status"=1 AND n."Seen" IS NOT NULL) GROUP BY st."Id" ORDER BY st."Status"`,[s.userId]);
  if(id) {
    z.string().uuid().parse(id);
    const notification=await one(`SELECT * FROM "UserNotifications" WHERE "Id"=$1 AND "IdentityUserId"=$2`,[id,s.userId]);assert(notification,404,'Уведомление не найдено');
    if(method==='GET')return notification;
    assert(action==='read' && method==='POST',405,'Операция недоступна');
    await transaction(async db=>{await db.query(`UPDATE "UserNotifications" SET "Seen"=coalesce("Seen",now()) WHERE "Id"=$1 AND "IdentityUserId"=$2`,[id,s.userId]);});return {ok:true};
  }
  const values:unknown[]=[s.userId],where=['n."IdentityUserId"=$1'];
  if(query.get('read')==='true')where.push('n."Seen" IS NOT NULL');
  else if(query.get('read')!=='all')where.push('n."Seen" IS NULL');
  if(query.get('categoryId')){values.push(z.string().uuid().parse(query.get('categoryId')));where.push(`n."NotificationCategoryId"=$${values.length}`);}
  if(query.get('search')){values.push('%'+query.get('search')+'%');where.push(`n."Title" ILIKE $${values.length}`);}
  const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??30);
  const from=`FROM "UserNotifications" n LEFT JOIN "NotificationCategories" c ON c."Id"=n."NotificationCategoryId" LEFT JOIN "AbpUsers" u ON u."Id"=n."CreatorId" WHERE ${where.join(' AND ')}`;
  const totalCount=(await one(`SELECT count(*)::integer AS count ${from}`,values))!.count;
  if(action==='count')return {totalCount};
  const sorts:Record<string,string>={created:'n."CreationTime"',title:'n."Title"',category:'c."Name"'};const sort=sorts[query.get('sort')??'created'];assert(sort,400,'Неизвестная сортировка');
  return {totalCount,items:await rows(`SELECT n.*,c."Name" AS "CategoryName",u."Name" AS "CreatorFirstName",u."Surname" AS "CreatorLastName" ${from} ORDER BY ${sort} ${query.get('direction')==='asc'?'ASC':'DESC'},n."Id" LIMIT $${values.length+1} OFFSET $${values.length+2}`,[...values,take,skip])};
}
