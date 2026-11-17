import 'server-only';
import { z } from 'zod';
import { rows, one, transaction } from '../db';
import { isHostAdmin, type Session } from '../auth/session';
import { assert } from '../errors';
export async function jobs(s:Session,id:string|undefined,method:string,query:URLSearchParams) {
  assert(isHostAdmin(s),403,'Требуется администратор');
  if(method==='GET') {
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??50);
    return {items:await rows(`SELECT "Id","Kind","Attempts","AvailableAt","LockedUntil","CompletedAt","LastError","CreatedAt" FROM "NextJobs" ORDER BY "CreatedAt" DESC LIMIT $1 OFFSET $2`,[take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count FROM "NextJobs"`))!.count};
  }
  z.string().uuid().parse(id);assert(method==='POST',405,'Метод не поддерживается');
  return transaction(async db=>{
    const job=await one(`SELECT * FROM "NextJobs" WHERE "Id"=$1 FOR UPDATE`,[id],db);assert(job,404,'Задание не найдено');assert(!job.CompletedAt,409,'Задание уже выполнено');assert(!job.LockedUntil||new Date(String(job.LockedUntil))<new Date(),409,'Задание выполняется');
    await db.query(`UPDATE "NextJobs" SET "Attempts"=0,"AvailableAt"=now(),"LockedUntil"=NULL,"LastError"=NULL WHERE "Id"=$1`,[id]);return {ok:true};
  });
}
