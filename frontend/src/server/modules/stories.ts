import 'server-only';
import { z } from 'zod';
import { one, rows, insert, transaction, update } from '../db';
import { requirePermission, requireStaff, type Session } from '../auth/session';
import { assert } from '../errors';
const storyInput=z.object({Name:z.string().max(1000),GroupName:z.string().max(1000),CoverImageUrl:z.string().max(2000),OrderIndex:z.number().int(),Status:z.number().int().min(0).max(1)}).strict();
const slideInput=z.object({ImageUrl:z.string().max(2000),OrderIndex:z.number().int()}).strict();
export async function stories(s:Session,id:string|undefined,slideId:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  if(!s.tenantId)requirePermission(s,'StoriesManagement');
  if(method==='GET') {
    if(!s.tenantId)requirePermission(s,'StoriesManagement.Get');
    const values:unknown[]=[],where:string[]=[];
    if(s.tenantId || query.get('active')==='true')where.push('"Status"=1');
    if(id){values.push(z.string().uuid().parse(id));where.push(`"Id"=$${values.length}`);}
    if(query.get('group')){values.push(query.get('group'));where.push(`"GroupName"=$${values.length}`);}
    const items=await rows(`SELECT * FROM "Stories" ${where.length?'WHERE '+where.join(' AND '):''} ORDER BY "OrderIndex","Id" LIMIT 200`,values);
    for(const item of items)item.Slides=await rows(`SELECT * FROM "Slides" WHERE "StoryId"=$1 ORDER BY "OrderIndex","Id"`,[item.Id]);
    if(id){assert(items[0],404,'История не найдена');return items[0];}return {items,totalCount:items.length};
  }
  requireStaff(s);requirePermission(s,'StoriesManagement.'+(method==='POST'?'Create':method==='DELETE'?'Delete':'Edit'));
  return transaction(async db=>{
    if(!id)return insert('Stories',storyInput.parse(body),db,s.userId);
    z.string().uuid().parse(id);assert(await one(`SELECT "Id" FROM "Stories" WHERE "Id"=$1 FOR UPDATE`,[id],db),404,'История не найдена');
    if(slideId) {
      if(slideId==='new')return insert('Slides',{...slideInput.parse(body),StoryId:id},db,s.userId);
      z.string().uuid().parse(slideId);assert(await one(`SELECT "Id" FROM "Slides" WHERE "Id"=$1 AND "StoryId"=$2`,[slideId,id],db),404,'Слайд не найден');
      if(method==='DELETE'){await db.query(`DELETE FROM "Slides" WHERE "Id"=$1`,[slideId]);return {ok:true};}
      return update('Slides',slideId,slideInput.partial().parse(body),db,s.userId);
    }
    if(method==='DELETE'){await db.query(`DELETE FROM "Stories" WHERE "Id"=$1`,[id]);return {ok:true};}
    return update('Stories',id,storyInput.partial().parse(body),db,s.userId);
  });
}
