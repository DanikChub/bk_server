import 'server-only';
import { Readable } from 'node:stream';
import { z } from 'zod';
import { insert, one, rows, transaction, pool, onRollback, type Row } from '../db';
import { requirePermission, scopeTicket, type Session } from '../auth/session';
import { assert } from '../errors';
import { putObject, getObject, deleteObject } from '../storage/s3';
import { ZipArchive } from 'archiver';

const maxBytes=50*1024*1024;
function disposition(name:string) {return `attachment; filename="download"; filename*=UTF-8''${encodeURIComponent(name.replace(/[\r\n\x00]/g,''))}`;}
async function visibleFiles(s:Session,id:string):Promise<Row[]> {
  await scopeTicket(s,id,pool());
  if(!s.tenantId)requirePermission(s,'TicketAttachments.Get');
  return rows(`SELECT f.* FROM "UploadedFiles" f WHERE EXISTS(SELECT 1 FROM "TicketAttachments" a WHERE a."TicketId"=$1 AND a."UploadedFileId"=f."Id")
    OR EXISTS(SELECT 1 FROM "TicketHistoryAttachments" a JOIN "TicketHistory" h ON h."Id"=a."TicketHistoryId" WHERE h."TicketId"=$1 AND a."UploadedFileId"=f."Id" AND NOT h."IsDeleted" ${s.tenantId?'AND NOT h."IsInternal" AND h."Type"<>5':''}) ORDER BY f."CreationTime",f."Id"`,[id]);
}
export async function downloadFile(s:Session,ticketId:string,fileId:string) {
  z.string().uuid().parse(fileId);
  const ticket=await scopeTicket(s,ticketId,pool());
  const file=(await visibleFiles(s,ticketId)).find(f=>f.Id===fileId);assert(file,404,'Файл не найден');
  const stream=await getObject('attachments',ticket.TenantId as string|null,fileId);
  return new Response(Readable.toWeb(stream) as ReadableStream,{headers:{'Content-Type':String(file.ContentType??'application/octet-stream'),'Content-Disposition':disposition(String(file.FileName??'download')),'Cache-Control':'private, no-store','X-Content-Type-Options':'nosniff'}});
}
export async function listFiles(s:Session,ticketId:string) {return visibleFiles(s,ticketId);}
export async function downloadZip(s:Session,ticketId:string) {
  const ticket=await scopeTicket(s,ticketId,pool()),files=await visibleFiles(s,ticketId);
  const archive=new ZipArchive({zlib:{level:5}});
  // Start production after a reader can consume the stream; errors abort the HTTP stream.
  void (async()=>{try {for(const file of files) {
    const source=await getObject('attachments',ticket.TenantId as string|null,String(file.Id));
    const safeName=String(file.FileName??file.Id).replace(/[\\/\x00-\x1f]/g,'_');
    archive.append(source,{name:`${file.Id}_${safeName}`});
  } await archive.finalize();} catch(e){archive.destroy(e as Error);} })();
  return new Response(Readable.toWeb(archive) as ReadableStream,{headers:{'Content-Type':'application/zip','Content-Disposition':disposition(`ticket-${ticketId}.zip`),'Cache-Control':'private, no-store'}});
}
export async function multipartForm(request:Request) {
  const length=Number(request.headers.get('content-length')??0);assert(length<=maxBytes+1024*1024,413,'Размер вложений превышает 50 МБ');
  const reader=request.body?.getReader();assert(reader,400,'Нет файлов');
  const chunks:Uint8Array[]=[];let size=0;
  try {for(;;){const part=await reader.read();if(part.done)break;size+=part.value.byteLength;assert(size<=maxBytes+1024*1024,413,'Размер вложений превышает 50 МБ');chunks.push(part.value);}}
  catch(e){await reader.cancel();throw e;}
  const copy=new Request(request.url,{method:'POST',headers:{'content-type':request.headers.get('content-type')??''},body:Buffer.concat(chunks)});
  return copy.formData();
}
export async function uploadFiles(s:Session,ticketId:string,request:Request|FormData) {
  const form=request instanceof FormData?request:await multipartForm(request),files=form.getAll('files');
  assert(files.length>0 && files.length<=30 && files.every(f=>typeof f!=='string'),400,'Укажите файлы');
  const historyId=form.get('historyId');if(historyId)z.string().uuid().parse(historyId);
  return transaction(async db=>{
    const ticket=await scopeTicket(s,ticketId,db,true);
    requirePermission(s,'TicketAttachments.Create');
    if(historyId) {
      const history=await one(`SELECT * FROM "TicketHistory" WHERE "Id"=$1 AND "TicketId"=$2 AND NOT "IsDeleted"`,[historyId,ticketId],db);assert(history,404,'Сообщение не найдено');
      if(s.tenantId)assert(history.CreatorId===s.userId && !history.IsInternal && history.Type!==5,403,'Недоступное сообщение');
    }
    const existing=await one(`SELECT coalesce(sum(f."SizeInBytes"),0)::text AS bytes FROM "UploadedFiles" f WHERE EXISTS(SELECT 1 FROM "TicketAttachments" a WHERE a."TicketId"=$1 AND a."UploadedFileId"=f."Id")`,[ticketId],db);
    const incoming=(files as File[]).reduce((sum,f)=>sum+f.size,0);assert(Number(existing!.bytes)+incoming<=maxBytes,413,'Размер вложений превышает 50 МБ');
    const result=[];
    for(const file of files as File[]) {
      assert(file.name.length<=1024 && file.name.length>0,400,'Недопустимое имя файла');
      const metadata=await insert('UploadedFiles',{FileName:file.name,OriginalFileName:file.name,ContentType:file.type||'application/octet-stream',SizeInBytes:file.size,Title:file.name,StorageProvider:'S3'},db,s.userId);
      const id=String(metadata.Id),tenantId=ticket.TenantId as string|null;
      onRollback(async()=>{
        // A failed COMMIT can have an uncertain outcome. Keep objects whose metadata committed.
        if(!await one(`SELECT "Id" FROM "UploadedFiles" WHERE "Id"=$1`,[id]))await deleteObject('attachments',tenantId,id);
      });
      await putObject('attachments',tenantId,id,Buffer.from(await file.arrayBuffer()),String(metadata.ContentType));
      if(historyId)await db.query(`INSERT INTO "TicketHistoryAttachments"("TicketHistoryId","UploadedFileId") VALUES($1,$2)`,[historyId,id]);
      else await db.query(`INSERT INTO "TicketAttachments"("TicketId","UploadedFileId") VALUES($1,$2)`,[ticketId,id]);
      result.push(metadata);
    }
    await db.query(`UPDATE "Tickets" SET "AttachmentsCount"=(SELECT count(*) FROM "TicketAttachments" WHERE "TicketId"=$1) WHERE "Id"=$1`,[ticketId]);
    return result;
  });
}
