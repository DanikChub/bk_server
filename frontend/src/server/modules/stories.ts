import 'server-only';
import { z } from 'zod';
import { one, rows, insert, transaction, update, type Executor } from '../db';
import { requirePermission, requireStaff, type Session } from '../auth/session';
import { assert } from '../errors';
import { enqueue } from '../jobs/queue';
async function cleanImage(db:Executor,id:unknown,url:unknown){
  const ids=new Set([String(id),String(url).split('?')[0].replace(/\/$/,'').split('/').at(-1)]);
  for(const key of ids)if(z.string().uuid().safeParse(key).success)await enqueue(db,'story.delete-image',{id:key});
}
const storyInput=z.object({Name:z.string().max(1000),GroupName:z.string().max(1000),CoverImageUrl:z.string().max(2000),OrderIndex:z.number().int(),Status:z.number().int().min(0).max(1)}).strict();
const slideInput=z.object({ImageUrl:z.string().max(2000),OrderIndex:z.number().int(),StoryId:z.string().uuid().optional()}).strict();
export async function stories(s:Session,id:string|undefined,slideId:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  if(!s.tenantId)requirePermission(s,'StoriesManagement');
  if(method==='GET') {
    if(!s.tenantId)requirePermission(s,'StoriesManagement.Get');
    const values:unknown[]=[],where:string[]=[];
    if(s.tenantId || query.get('active')==='true')where.push('"Status"=1');
    if(id){values.push(z.string().uuid().parse(id));where.push(`"Id"=$${values.length}`);}
    if(query.get('group')){values.push(query.get('group'));where.push(`"GroupName"=$${values.length}`);}
    const clause=where.length?' WHERE '+where.join(' AND '):'';
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??100);
    if(slideId){
      assert(id && await one(`SELECT "Id" FROM "Stories"${clause}`,values),404,'История не найдена');
      if(slideId!=='new'){z.string().uuid().parse(slideId);const slide=await one(`SELECT * FROM "Slides" WHERE "Id"=$1 AND "StoryId"=$2`,[slideId,id]);assert(slide,404,'Слайд не найден');return slide;}
      return {items:await rows(`SELECT * FROM "Slides" WHERE "StoryId"=$1 ORDER BY "OrderIndex","Id" LIMIT $2 OFFSET $3`,[id,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count FROM "Slides" WHERE "StoryId"=$1`,[id]))!.count};
    }
    const sorts:Record<string,string>={order:'"OrderIndex"',name:'"Name"',group:'"GroupName"'};const sort=sorts[query.get('sort')??'order'];assert(sort,400,'Неизвестная сортировка');
    const items=await rows(`SELECT * FROM "Stories"${clause} ORDER BY ${sort} ${query.get('direction')==='desc'?'DESC':'ASC'},"Id" LIMIT $${values.length+1} OFFSET $${values.length+2}`,[...values,id?1:take,id?0:skip]);
    for(const item of items)item.Slides=await rows(`SELECT * FROM "Slides" WHERE "StoryId"=$1 ORDER BY "OrderIndex","Id"`,[item.Id]);
    if(id){assert(items[0],404,'История не найдена');return items[0];}return {items,totalCount:(await one(`SELECT count(*)::integer AS count FROM "Stories"${clause}`,values))!.count};
  }
  requireStaff(s);requirePermission(s,'StoriesManagement.'+(method==='POST'?'Create':method==='DELETE'?'Delete':'Edit'));
  return transaction(async db=>{
    if(!id)return insert('Stories',storyInput.parse(body),db,s.userId);
    z.string().uuid().parse(id);const story=await one(`SELECT * FROM "Stories" WHERE "Id"=$1 FOR UPDATE`,[id],db);assert(story,404,'История не найдена');
    if(slideId) {
      if(slideId==='new'){const data=slideInput.parse(body);assert(!data.StoryId||data.StoryId===id,400,'История не совпадает с маршрутом');return insert('Slides',{...data,StoryId:id},db,s.userId);}
      z.string().uuid().parse(slideId);const slide=await one(`SELECT * FROM "Slides" WHERE "Id"=$1 AND "StoryId"=$2 FOR UPDATE`,[slideId,id],db);assert(slide,404,'Слайд не найден');
      if(method==='DELETE'){await db.query(`DELETE FROM "Slides" WHERE "Id"=$1`,[slideId]);await cleanImage(db,slideId,slide.ImageUrl);return {ok:true};}
      const data=slideInput.partial().refine(v=>Object.keys(v).length>0,'Нет изменений').parse(body);
      if(data.StoryId)assert(await one(`SELECT "Id" FROM "Stories" WHERE "Id"=$1`,[data.StoryId],db),404,'Целевая история не найдена');
      if(data.ImageUrl!==undefined && data.ImageUrl!==slide.ImageUrl)await cleanImage(db,slideId,slide.ImageUrl);
      return update('Slides',slideId,data,db,s.userId);
    }
    if(method==='DELETE'){
      const slides=await rows(`SELECT "Id","ImageUrl" FROM "Slides" WHERE "StoryId"=$1`,[id],db);
      await db.query(`DELETE FROM "Stories" WHERE "Id"=$1`,[id]);
      for(const slide of slides)await cleanImage(db,slide.Id,slide.ImageUrl);
      await cleanImage(db,id,story.CoverImageUrl);return {ok:true};
    }
    const data=storyInput.partial().refine(v=>Object.keys(v).length>0,'Нет изменений').parse(body);
    if(data.CoverImageUrl!==undefined && data.CoverImageUrl!==story.CoverImageUrl)await cleanImage(db,id,story.CoverImageUrl);
    return update('Stories',id,data,db,s.userId);
  });
}
