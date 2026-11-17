import 'server-only';
import { z } from 'zod';
import { one, insert, update, transaction } from '../db';
const envelope=z.object({Entity:z.object({Id:z.string().uuid(),Name:z.string().max(1000).optional()})});
export async function applyTenantEvent(kind:'created'|'deleted',body:unknown,messageId:string) {
  const raw=body as {Entity?:{Id?:string;Name?:string};entity?:{id?:string;name?:string}};
  const data=envelope.parse(raw?.Entity?raw:{Entity:{Id:raw?.entity?.id,Name:raw?.entity?.name}});
  return transaction(async db=>{
    const accepted=await one(`INSERT INTO "NextInboxMessages"("MessageId") VALUES($1) ON CONFLICT DO NOTHING RETURNING "MessageId"`,[messageId],db);
    if(!accepted)return {duplicate:true};
    const id=data.Entity.Id,existing=await one(`SELECT "Id" FROM "TenantProfiles" WHERE "Id"=$1`,[id],db);
    if(kind==='created') {
      if(!existing)await insert('TenantProfiles',{Id:id,ShortName:data.Entity.Name??'',LongName:'',Address:'',IsActive:false},db);
    }else if(existing)await update('TenantProfiles',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:null},db);
    return {duplicate:false};
  });
}
