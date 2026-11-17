import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { bootstrap } from '../src/server/bootstrap';
import { pool, one, rows, transaction, insert } from '../src/server/db';
import { customers } from '../src/server/modules/customers';
import { users } from '../src/server/modules/users';
import { contracts } from '../src/server/modules/contracts';
import { createTicket, ticketDetails, mutateTicket, listTickets } from '../src/server/modules/tickets';
import { notifications } from '../src/server/modules/notifications';
import { type Session } from '../src/server/auth/session';
import { createServer, type Server } from 'node:http';
import { uploadFiles, downloadFile, downloadZip } from '../src/server/modules/files';
import { login } from '../src/server/auth/session';
import { portalSnapshot } from '../src/server/modules/portal';
import { queueJob } from '../src/server/jobs/queue';
import { runJob } from '../src/server/jobs/worker';
import { applyTenantEvent } from '../src/server/jobs/tenant-events';
import { randomUUID } from 'node:crypto';

const enabled=Boolean(process.env.BACKEND_TEST_DATABASE_URL);
if(enabled)process.env.DATABASE_URL=process.env.BACKEND_TEST_DATABASE_URL;
let admin:Session,client:Session,other:Session,customerId:string,contractId:string,ticketId:string;
let s3Server:Server;
const objects=new Map<string,Buffer>();
const q=new URLSearchParams();
before(async()=>{
  if(!enabled)return;
  assert.match(process.env.DATABASE_URL!,/backend_test/,'Integration tests must use a dedicated backend_test database');
  const result=await bootstrap({userName:'integration-admin',email:'admin@example.test',password:'Integration-Admin-Password!'});
  admin={userId:String((result as {id:string}).id),tenantId:null,userName:'integration-admin',roles:['admin'],permissions:[],specialistId:null};
  s3Server=createServer(async(req,res)=>{
    const key=new URL(req.url!,'http://localhost').pathname;
    if(req.method==='PUT'){const parts:Buffer[]=[];for await(const part of req)parts.push(Buffer.from(part));objects.set(key,Buffer.concat(parts));res.writeHead(200);res.end();}
    else if(req.method==='GET' && objects.has(key)){const data=objects.get(key)!;res.writeHead(200,{'Content-Length':data.length,'Content-Type':'application/octet-stream'});res.end(data);}
    else if(req.method==='DELETE'){objects.delete(key);res.writeHead(204);res.end();}
    else{res.writeHead(404);res.end();}
  });
  await new Promise<void>(resolve=>s3Server.listen(0,'127.0.0.1',resolve));
  const port=(s3Server.address() as {port:number}).port;
  Object.assign(process.env,{S3_ENDPOINT:`http://127.0.0.1:${port}`,S3_REGION:'us-east-1',S3_ACCESS_KEY_ID:'test-key',S3_SECRET_ACCESS_KEY:'test-secret',S3_FORCE_PATH_STYLE:'true',S3_ATTACHMENTS_BUCKET:'attachments'});
});
after(async()=>{if(enabled){await pool().end();if(s3Server)await new Promise<void>((resolve,reject)=>s3Server.close(error=>error?reject(error):resolve()));}});

test('customer creation atomically creates tenant, role, grants, contract statuses and outbox', {skip:!enabled},async()=>{
  const result=await customers(admin,undefined,undefined,'POST',{LongName:'Тестовый клиент',ShortName:'Тест'},q) as {Id:string};customerId=result.Id;
  assert.ok(await one(`SELECT "Id" FROM "AbpTenants" WHERE "Id"=$1`,[customerId]));
  assert.equal((await rows(`SELECT * FROM "ContractStatuses" WHERE "TenantId"=$1`,[customerId])).length,4);
  assert.equal((await rows(`SELECT * FROM "AbpPermissionGrants" WHERE "TenantId"=$1`,[customerId])).length,11);
  assert.equal((await rows(`SELECT * FROM "NextJobs" WHERE "Kind"='tenant.created'`)).length,1);
  const created=await users(admin,undefined,undefined,'POST',{userName:'client',password:'Client-Password!',email:'client@example.test',tenantId:customerId},q) as {id:string};
  client={userId:created.id,tenantId:customerId,userName:'client',roles:['Клиент'],permissions:['TicketAttachments.Create'],specialistId:null};
  const second=await customers(admin,undefined,undefined,'POST',{LongName:'Другой клиент',ShortName:'Другой'},q) as {Id:string};
  other={...client,tenantId:second.Id};
});
test('contract, type, status and section produce a real ticket and trigger aggregates',{skip:!enabled},async()=>{
  const st=await one(`SELECT "Id" FROM "ContractStatuses" WHERE "TenantId"=$1 AND "Code"='in progress'`,[customerId]);
  const contract=await contracts(admin,undefined,undefined,'POST',{name:'Договор №1',tenantId:customerId,startsAt:'2026-01-01T00:00:00Z',endsAt:'2099-12-31T00:00:00Z',statusId:st!.Id},q) as {Id:string};contractId=contract.Id;
  const type=await one(`SELECT "Id" FROM "TicketTypes" ORDER BY "Id" LIMIT 1`),section=await one(`SELECT "Id" FROM "TicketSections" LIMIT 1`);
  const ticket=await createTicket(client,{subject:'Вопрос',description:'Описание',ticketTypeId:type!.Id,ticketSectionId:section!.Id,contractId});ticketId=String(ticket.Id);
  assert.equal(ticket.TenantId,customerId);
  const counts=await rows(`SELECT * FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1`,[contractId]);assert.equal(counts.length,1);assert.equal(counts[0].Count,1);
  assert.equal((await listTickets(client,q)).totalCount,1);
  assert.equal((await listTickets(other,q)).totalCount,0);
  await assert.rejects(()=>ticketDetails(other,ticketId),/не найдена/);
});
test('internal messages never appear in client details; client cannot forge privileged reply fields',{skip:!enabled},async()=>{
  await mutateTicket(admin,ticketId,'reply',{description:'Секретная заметка',internal:true});
  await mutateTicket(client,ticketId,'reply',{description:'Сообщение клиента'});
  const staff=await ticketDetails(admin,ticketId),publicView=await ticketDetails(client,ticketId);
  assert.ok(staff.history.some(h=>h.Description==='Секретная заметка'));
  assert.equal(publicView.history.some(h=>h.Description==='Секретная заметка'),false);
  await assert.rejects(()=>mutateTicket(client,ticketId,'reply',{description:'attack',internal:true}),/Недоступные поля/);
});
test('response, status change and limit write roll back as a unit on invalid reference',{skip:!enabled},async()=>{
  const before=await ticketDetails(admin,ticketId),st=await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"='Answered'`);
  await assert.rejects(()=>mutateTicket(admin,ticketId,'reply',{description:'Must rollback',statusId:st!.Id,constraintTypeId:'00000000-0000-0000-0000-000000000001',spent:2}));
  const after=await ticketDetails(admin,ticketId);
  assert.equal(after.history.length,before.history.length);assert.equal(after.TicketStatusId,before.TicketStatusId);
});
test('limit writes use original PostgreSQL trigger, closed ticket reopens on customer reply',{skip:!enabled},async()=>{
  const constraint=await one(`SELECT "Id" FROM "ConstraintTypes" LIMIT 1`);
  await contracts(admin,contractId,'limits','POST',{constraintTypeId:constraint!.Id,max:10},q);
  await mutateTicket(admin,ticketId,'reply',{description:'Ответ специалиста',constraintTypeId:constraint!.Id,spent:3});
  const aggregate=await one(`SELECT "Sum","MaxCount" FROM "ContractConstraintsByMonths" WHERE "ContractId"=$1 AND "ConstraintTypeId"=$2`,[contractId,constraint!.Id]);
  assert.equal(aggregate!.Sum,3);assert.equal(aggregate!.MaxCount,10);
  const closed=await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"='Answered'`),renewed=await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"='Renewed'`);
  await mutateTicket(admin,ticketId,'status',{statusId:closed!.Id});
  await mutateTicket(client,ticketId,'reply',{description:'Дополнительный вопрос'});
  assert.equal((await ticketDetails(client,ticketId)).TicketStatusId,renewed!.Id);
});
test('favorites are scoped and idempotent; drafts are private to their creator',{skip:!enabled},async()=>{
  await mutateTicket(client,ticketId,'favorite',{favorite:true});await mutateTicket(client,ticketId,'favorite',{favorite:true});
  assert.equal((await listTickets(client,new URLSearchParams({favorite:'true'}))).totalCount,1);
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`);
  const draft=await createTicket(client,{subject:'Черновик',description:'',ticketTypeId:type!.Id,contractId,draft:true});
  const draftStatus=await one(`SELECT "IsPublic" FROM "TicketStatuses" WHERE "Id"=$1`,[draft.TicketStatusId]);
  // The original seeder marks Draft public; the application must still enforce creator ownership.
  assert.equal(draftStatus!.IsPublic,true);
  await assert.rejects(()=>mutateTicket(other,String(draft.Id),'draft',{subject:'attack',description:'',ticketTypeId:type!.Id,draft:true}),/не найдена/);
});
test('notification reads cannot cross user boundaries',{skip:!enabled},async()=>{
  const notification=await transaction(db=>insert('UserNotifications',{IdentityUserId:admin.userId,Title:'Личное',Url:'/'},db,admin.userId));
  await assert.rejects(()=>notifications(client,String(notification.Id),undefined,'GET',q),/не найдено/);
});
test('legacy password login persists failures, isolates tenant and stores only hashed session tokens',{skip:!enabled},async()=>{
  await assert.rejects(()=>login('client','wrong','Тест','test'),/Неверные/);
  assert.equal((await one(`SELECT "AccessFailedCount" FROM "AbpUsers" WHERE "Id"=$1`,[client.userId]))!.AccessFailedCount,1);
  await assert.rejects(()=>login('client','Client-Password!',null,'test'),/Неверные/);
  const token=await login('client','Client-Password!','Тест','test');assert.match(token,/^[a-f0-9]{64}$/);
  const stored=await one(`SELECT "TokenHash","UserId" FROM "NextSessions" WHERE "UserId"=$1`,[client.userId]);assert.notEqual(stored!.TokenHash,token);
  assert.equal((await one(`SELECT "AccessFailedCount" FROM "AbpUsers" WHERE "Id"=$1`,[client.userId]))!.AccessFailedCount,0);
});
test('S3 upload and streamed download preserve legacy tenant paths; ZIP and tenant checks work',{skip:!enabled},async()=>{
  const form=new FormData();form.append('files',new File(['Содержимое файла'],'вложение.txt',{type:'text/plain'}));
  const uploaded=await uploadFiles(client,ticketId,new Request('http://localhost/upload',{method:'POST',body:form}));
  assert.equal(uploaded.length,1);const id=String(uploaded[0].Id);
  assert.ok(objects.has(`/attachments/tenants/${customerId}/${id}`));
  const response=await downloadFile(client,ticketId,id);assert.equal(await response.text(),'Содержимое файла');
  assert.match(response.headers.get('content-disposition')!,/filename\*=UTF-8/);
  await assert.rejects(()=>downloadFile(other,ticketId,id),/не найдена/);
  const zip=await downloadZip(client,ticketId),buffer=Buffer.from(await zip.arrayBuffer());assert.equal(buffer.subarray(0,2).toString(),'PK');assert.ok(buffer.includes(Buffer.from(id)));
  const snapshot=await portalSnapshot(client);assert.ok(snapshot.tickets.some(t=>t.id===ticketId && t.attachments.some(f=>f.id===id)));
  assert.equal(snapshot.tickets.flatMap(t=>t.messages).some(m=>m.text==='Секретная заметка'),false);
});
test('failed attachment SQL write compensates the object already uploaded to S3',{skip:!enabled},async()=>{
  await rows(`CREATE FUNCTION backend_test_reject_attachment() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'test failure'; END $$`);
  await rows(`CREATE TRIGGER backend_test_reject_attachment BEFORE INSERT ON "TicketAttachments" FOR EACH ROW EXECUTE FUNCTION backend_test_reject_attachment()`);
  const count=objects.size,before=(await one(`SELECT count(*)::integer AS count FROM "UploadedFiles"`))!.count;
  try{
    const form=new FormData();form.append('files',new File(['Rollback me'],'rollback.txt'));
    await assert.rejects(()=>uploadFiles(client,ticketId,new Request('http://localhost/upload',{method:'POST',body:form})),/test failure/);
    assert.equal(objects.size,count);assert.equal((await one(`SELECT count(*)::integer AS count FROM "UploadedFiles"`))!.count,before);
  }finally{await rows(`DROP TRIGGER backend_test_reject_attachment ON "TicketAttachments"`);await rows(`DROP FUNCTION backend_test_reject_attachment()`);}
});
test('ticket creation and reply roll back with failed attachment writes in the same transaction',{skip:!enabled},async()=>{
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`);
  const before=await ticketDetails(admin,ticketId),count=objects.size;
  const attachment=()=>{const form=new FormData();form.append('files',new File(['Rollback entire command'],'atomic.txt'));return form;};
  await assert.rejects(()=>transaction(async()=>{
    const ticket=await createTicket(client,{subject:'Atomic rollback',description:'Описание',ticketTypeId:type!.Id,contractId});
    await uploadFiles(client,String(ticket.Id),attachment());
    throw new Error('atomic failure');
  }),/atomic failure/);
  assert.equal((await rows(`SELECT "Id" FROM "Tickets" WHERE "Subject"='Atomic rollback'`)).length,0);
  await assert.rejects(()=>transaction(async()=>{
    const history=await mutateTicket(client,ticketId,'reply',{description:'Atomic reply rollback'}) as {Id:string};
    const form=attachment();form.set('historyId',history.Id);await uploadFiles(client,ticketId,form);
    throw new Error('atomic failure');
  }),/atomic failure/);
  assert.equal((await ticketDetails(admin,ticketId)).history.length,before.history.length);
  assert.equal(objects.size,count);
});
test('worker refreshes organization activity and claims its job atomically',{skip:!enabled},async()=>{
  const id=await queueJob('customers.refresh-active',{});
  await rows(`UPDATE "NextJobs" SET "AvailableAt"='2000-01-01' WHERE "Id"=$1`,[id]);
  assert.equal(await runJob(),true);
  assert.equal((await one(`SELECT "IsActive" FROM "TenantProfiles" WHERE "Id"=$1`,[customerId]))!.IsActive,true);
  const job=await one(`SELECT "CompletedAt","Attempts","LockedUntil" FROM "NextJobs" WHERE "Id"=$1`,[id]);assert.ok(job!.CompletedAt);assert.equal(job!.Attempts,1);assert.equal(job!.LockedUntil,null);
});
test('deleting a customer removes its profiles, soft-deletes tickets/contracts and revokes access together',{skip:!enabled},async()=>{
  const owner=await users(admin,undefined,undefined,'POST',{userName:'removed-client',password:'Removed-Client-Password!',email:'removed@example.test',tenantId:other.tenantId},q) as {id:string};
  const status=await one(`SELECT "Id" FROM "ContractStatuses" WHERE "TenantId"=$1 LIMIT 1`,[other.tenantId]);
  const contract=await contracts(admin,undefined,undefined,'POST',{name:'Удаляемый договор',tenantId:other.tenantId,startsAt:'2026-01-01T00:00:00Z',endsAt:'2099-12-31T00:00:00Z',statusId:status!.Id},q) as {Id:string};
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`);
  const ticket=await createTicket({...other,userId:owner.id},{subject:'Удаляемая заявка',description:'Описание',ticketTypeId:type!.Id,contractId:contract.Id});
  await customers(admin,other.tenantId!,undefined,'DELETE',undefined,q);
  assert.equal((await one(`SELECT "IsDeleted" FROM "Tickets" WHERE "Id"=$1`,[ticket.Id]))!.IsDeleted,true);
  assert.equal((await one(`SELECT "IsDeleted" FROM "Contracts" WHERE "Id"=$1`,[contract.Id]))!.IsDeleted,true);
  assert.equal((await rows(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "TenantId"=$1`,[other.tenantId])).length,0);
  assert.equal((await one(`SELECT "IsActive" FROM "AbpUsers" WHERE "Id"=$1`,[owner.id]))!.IsActive,false);
  await assert.rejects(()=>login('removed-client','Removed-Client-Password!','Другой','test'),/Неверные/);
});
test('tenant event handlers accept both serializer cases and deduplicate broker redelivery',{skip:!enabled},async()=>{
  const id=randomUUID(),messageId=randomUUID();
  assert.equal((await applyTenantEvent('created',{entity:{id,name:'Из события'}},messageId)).duplicate,false);
  assert.equal((await applyTenantEvent('created',{Entity:{Id:id,Name:'Не перезаписывать'}},messageId)).duplicate,true);
  assert.equal((await one(`SELECT "ShortName" FROM "TenantProfiles" WHERE "Id"=$1`,[id]))!.ShortName,'Из события');
  await applyTenantEvent('deleted',{Entity:{Id:id}},randomUUID());
  assert.equal((await one(`SELECT "IsDeleted" FROM "TenantProfiles" WHERE "Id"=$1`,[id]))!.IsDeleted,true);
});
