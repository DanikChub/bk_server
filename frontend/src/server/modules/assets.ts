import 'server-only';
import { Readable } from 'node:stream';
import { z } from 'zod';
import { insert, one, transaction, update, onRollback } from '../db';
import { type Session, requirePermission, requireStaff } from '../auth/session';
import { assert } from '../errors';
import { putObject, getObject, deleteObject, type Container } from '../storage/s3';
import { enqueue, queueJob } from '../jobs/queue';
async function imageInput(request:Request) {
  const reader=request.body?.getReader();assert(reader,400,'Изображение отсутствует');
  const chunks:Uint8Array[]=[];let length=0;
  try{for(;;){const part=await reader.read();if(part.done)break;length+=part.value.byteLength;assert(length<=6*1024*1024,413,'Изображение больше 5 МБ');chunks.push(part.value);}}catch(error){await reader.cancel();throw error;}
  const requestCopy=new Request(request.url,{method:'POST',headers:{'content-type':request.headers.get('content-type')??''},body:Buffer.concat(chunks)});
  const form=await requestCopy.formData(),file=form.get('file');assert(file && typeof file!=='string' && file.size<=5*1024*1024,400,'Укажите изображение до 5 МБ');
  const data=Buffer.from(await file.arrayBuffer());
  const type=data.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10]))?'image/png':data[0]===255&&data[1]===216&&data[2]===255?'image/jpeg':data.subarray(0,4).toString()==='RIFF'&&data.subarray(8,12).toString()==='WEBP'?'image/webp':null;
  assert(type,400,'Поддерживаются PNG, JPEG и WebP');return {file,data,type};
}
export async function asset(s:Session,kind:'avatar'|'stories',method:string,request:Request,id?:string,target?:{userId:string;employee:boolean}) {
  const container:Container=kind==='stories'?'stories':'public';
  let tenantId:string|null=null;
  let profile:Record<string,unknown>|undefined;
  const profileTable=target?(target.employee?'EmployeeProfiles':'CustomerUserProfiles'):s.tenantId?'CustomerUserProfiles':'EmployeeProfiles';
  if(kind==='avatar') {
    if(target){z.string().uuid().parse(target.userId);requireStaff(s);requirePermission(s,(target.employee?'EmployeeProfileManagement':'UserManagement')+'.'+(method==='GET'?'Get':'Edit'));}
    profile=await one(`SELECT p.*,u."TenantId" AS tenant FROM "${profileTable}" p JOIN "AbpUsers" u ON u."Id"=p."IdentityUserId" WHERE p."IdentityUserId"=$1 AND NOT u."IsDeleted"`,[target?.userId??s.userId]);assert(profile,404,'Профиль не найден');
    tenantId=profile.tenant as string|null;id=profile.UserAvatarFileId as string|undefined;
  }else if(method!=='GET'){requireStaff(s);requirePermission(s,'StoriesManagement.Create');}
  if(method==='GET') {
    assert(id,404,'Изображение отсутствует');z.string().uuid().parse(id);
    const stream=await getObject(container,tenantId,id);
    const metadata=await one(`SELECT "ContentType" FROM "UploadedFiles" WHERE "Id"=$1`,[id]);
    const contentType=String(metadata?.ContentType??'image/jpeg');
    return new Response(Readable.toWeb(stream) as ReadableStream,{headers:{'Content-Type':contentType,'X-Content-Type-Options':'nosniff','Cache-Control':'private, no-store'}});
  }
  const {file,data,type}=await imageInput(request);
  return transaction(async db=>{
    const stored=await insert('UploadedFiles',{FileName:file.name,OriginalFileName:file.name,ContentType:type,SizeInBytes:data.length,Title:file.name,StorageProvider:'S3'},db,s.userId);const storedId=String(stored.Id);
    onRollback(async()=>{if(!await one(`SELECT "Id" FROM "UploadedFiles" WHERE "Id"=$1`,[storedId]))try{await deleteObject(container,tenantId,storedId);}catch{await queueJob(container==='stories'?'story.delete-image':'file.delete-if-unreferenced',{id:storedId,tenantId,container},'rollback-image:'+storedId);}});
    await putObject(container,tenantId,storedId,data,type);
    if(kind==='avatar'){
      // Lock/re-read so two simultaneous replacements also collect the intermediate image.
      const current=await one(`SELECT "UserAvatarFileId" FROM "${profileTable}" WHERE "Id"=$1 FOR UPDATE`,[profile!.Id],db);
      await update(profileTable,profile!.Id,{UserAvatarFileId:stored.Id},db,s.userId);
      if(current?.UserAvatarFileId)await enqueue(db,'file.delete-if-unreferenced',{id:current.UserAvatarFileId,tenantId,container});
    }
    return {id:stored.Id,url:kind==='avatar'?target?`/api/v1/${target.employee?'employees':'users'}/${target.userId}/avatar`:'/api/v1/profile/avatar':`/api/v1/assets/stories/${stored.Id}`};
  });
}
