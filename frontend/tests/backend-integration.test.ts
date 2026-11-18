import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { bootstrap } from '../src/server/bootstrap';
import { pool, one, rows, transaction, insert, update } from '../src/server/db';
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
import { stories } from '../src/server/modules/stories';
import { putObject } from '../src/server/storage/s3';
import { statusCounts, spentTime, ticketSpent } from '../src/server/modules/statistics';
import { asset } from '../src/server/modules/assets';
import { tags } from '../src/server/modules/tags';
import { requestPasswordReset, resetPassword } from '../src/server/auth/recovery';
import { sendEmail } from '../src/server/integrations/email';
import { createServer as createTcpServer } from 'node:net';
import { messages } from '../src/server/modules/messages';
import { events } from '../src/server/modules/events';
import { spreadsheet } from '../src/server/modules/reports';
import ExcelJS from 'exceljs';
import { Readable } from 'node:stream';
import { productionCalendar } from '../src/server/integrations/calendar';
import { content } from '../src/server/integrations/content';
import { legacyContent } from '../src/server/integrations/legacy-content';

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
  Object.assign(process.env,{S3_ENDPOINT:`http://127.0.0.1:${port}`,S3_REGION:'us-east-1',S3_ACCESS_KEY_ID:'test-key',S3_SECRET_ACCESS_KEY:'test-secret',S3_FORCE_PATH_STYLE:'true',S3_ATTACHMENTS_BUCKET:'attachments',S3_STORIES_BUCKET:'stories',S3_PUBLIC_BUCKET:'public'});
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
test('reply notifications are transactional, target the creator and contain no internal message text',{skip:!enabled},async()=>{
  const count=async()=>Number((await one(`SELECT count(*)::integer AS n FROM "UserNotifications" WHERE "IdentityUserId"=$1`,[client.userId]))!.n);
  const before=await count();await mutateTicket(admin,ticketId,'reply',{description:'Private secret',internal:true});
  assert.equal(await count(),before+1);
  const notification=await one(`SELECT * FROM "UserNotifications" WHERE "IdentityUserId"=$1 ORDER BY "CreationTime" DESC LIMIT 1`,[client.userId]);assert.equal(notification!.Title,`Ответ по заявке ${ticketId}`);assert.equal(notification!.Url,`/client/ticket?id=${ticketId}`);
  await mutateTicket(client,ticketId,'reply',{description:'Self message'});assert.equal(await count(),before+1);
  await assert.rejects(()=>mutateTicket(admin,ticketId,'reply',{description:'Must rollback notification',constraintTypeId:randomUUID(),spent:1}));assert.equal(await count(),before+1);
});
test('full ticket edit and compound panel validate input and roll back all effects',{skip:!enabled},async()=>{
  const before=await ticketDetails(admin,ticketId);
  await mutateTicket(admin,ticketId,'update',{subject:'Отредактированная тема'});assert.equal((await ticketDetails(admin,ticketId)).Subject,'Отредактированная тема');
  await assert.rejects(()=>mutateTicket(admin,ticketId,'update',{subject:'forged',CreatorId:admin.userId}));
  const eventCount=Number((await one(`SELECT count(*)::integer AS n FROM "Events" WHERE "TicketId"=$1`,[ticketId]))!.n);
  await assert.rejects(()=>mutateTicket(admin,ticketId,'panel',{event:{title:'Rollback event',date:new Date().toISOString()},specialistId:randomUUID()}));
  assert.equal(Number((await one(`SELECT count(*)::integer AS n FROM "Events" WHERE "TicketId"=$1`,[ticketId]))!.n),eventCount);
  await mutateTicket(admin,ticketId,'update',{subject:before.Subject});
});
test('manager lists combine legacy additional managers and current responsible profiles',{skip:!enabled},async()=>{
  const manager=await users(admin,undefined,undefined,'POST',{userName:'test-manager',password:'Manager-Password!',email:'manager@example.test'},q) as {id:string;profileId:string};
  await rows(`INSERT INTO "TenantProfileManagers"("TenantProfileId","ManagerId") VALUES($1,$2)`,[customerId,manager.profileId]);
  const s:Session={...admin,userId:manager.id,roles:['Менеджер Отдела Продаж'],permissions:['Ticket','Ticket.Get','Contract','Contract.Get'],specialistId:null};
  const tickets=await listTickets(s,q);assert.ok(tickets.items.length);assert.ok(tickets.items.every(t=>t.TenantId===customerId));
  const page=await contracts(s,undefined,undefined,'GET',undefined,q) as {items:{TenantId:string}[]};assert.ok(page.items.length);assert.ok(page.items.every(c=>c.TenantId===customerId));
  const explicit=await listTickets(admin,new URLSearchParams({managerId:manager.profileId}));assert.equal(explicit.totalCount,tickets.totalCount);
  const counts=await statusCounts(s,new URLSearchParams({favorite:'true'}));assert.equal(counts.reduce((sum,c)=>sum+Number(c.count),0),0);
});
test('history ZIP selection cannot expose internal or foreign history files',{skip:!enabled},async()=>{
  const history=await mutateTicket(admin,ticketId,'reply',{description:'Hidden attachment',internal:true}) as {Id:string};
  const form=new FormData();form.set('historyId',history.Id);form.append('files',new File(['Internal file'],'internal.txt'));
  const files=await uploadFiles(admin,ticketId,form);
  await assert.rejects(()=>downloadZip(client,ticketId,{kind:'history',historyId:history.Id}),/не найдено/);
  await assert.rejects(()=>downloadZip(admin,ticketId,{kind:'history',historyId:randomUUID()}),/не найдено/);
  const zip=Buffer.from(await (await downloadZip(admin,ticketId,{kind:'history',historyId:history.Id})).arrayBuffer());assert.ok(zip.includes(Buffer.from(String(files[0].Id))));
  const ticketZip=Buffer.from(await (await downloadZip(admin,ticketId,{kind:'ticket'})).arrayBuffer());assert.equal(ticketZip.includes(Buffer.from(String(files[0].Id))),false);
});
test('stories paginate, isolate inactive content and garbage collection retains shared images',{skip:!enabled},async()=>{
  const imageId=randomUUID(),url='/api/v1/assets/stories/'+imageId;
  await putObject('stories',null,imageId,Buffer.from('image'),'image/png');
  const create=async(name:string,Status=1)=>stories(admin,undefined,undefined,'POST',{Name:name,GroupName:'test',CoverImageUrl:url,OrderIndex:0,Status},q) as Promise<{Id:string}>;
  const first=await create('first'),second=await create('second',0);
  const page=await stories(admin,undefined,undefined,'GET',undefined,new URLSearchParams({group:'test',take:'1'})) as {items:unknown[];totalCount:number};assert.equal(page.items.length,1);assert.equal(page.totalCount,2);
  await assert.rejects(()=>stories(client,second.Id,undefined,'GET',undefined,q),/не найдена/);
  const slide=await stories(admin,first.Id,'new','POST',{ImageUrl:url,OrderIndex:1},q) as {Id:string};
  const slides=await stories(admin,first.Id,'new','GET',undefined,q) as {totalCount:number};assert.equal(slides.totalCount,1);
  await stories(admin,first.Id,slide.Id,'PATCH',{StoryId:second.Id},q);
  await stories(admin,first.Id,undefined,'DELETE',undefined,q);
  const collect=async()=>{const job=await one(`UPDATE "NextJobs" SET "AvailableAt"='1990-01-01' WHERE "Id"=(SELECT "Id" FROM "NextJobs" WHERE "Kind"='story.delete-image' AND "Payload"->>'id'=$1 AND "CompletedAt" IS NULL LIMIT 1) RETURNING "Id"`,[imageId]);assert.ok(job);await runJob();assert.ok((await one(`SELECT "CompletedAt" FROM "NextJobs" WHERE "Id"=$1`,[job.Id]))!.CompletedAt);};
  await collect();assert.ok(objects.has('/stories/host/'+imageId));
  await stories(admin,second.Id,undefined,'DELETE',undefined,q);await collect();assert.equal(objects.has('/stories/host/'+imageId),false);
});
test('administrative avatar operations check permissions and preserve public container tenant keys',{skip:!enabled},async()=>{
  const png=Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a6K0AAAAASUVORK5CYII=','base64');
  const form=new FormData();form.set('file',new File([png],'avatar.png',{type:'image/png'}));
  await assert.rejects(()=>asset(client,'avatar','POST',new Request('http://localhost/upload',{method:'POST',body:form}),undefined,{userId:admin.userId,employee:true}),/сотрудникам/);
  const uploaded=await asset(admin,'avatar','POST',new Request('http://localhost/upload',{method:'POST',body:form}),undefined,{userId:client.userId,employee:false}) as {id:string};
  assert.ok(objects.has(`/public/tenants/${customerId}/${uploaded.id}`));
  const downloaded=await asset(admin,'avatar','GET',new Request('http://localhost/avatar'),undefined,{userId:client.userId,employee:false}) as Response;
  assert.equal(Buffer.compare(Buffer.from(await downloaded.arrayBuffer()),png),0);
  const identity=await users(admin,undefined,undefined,'POST',{userName:'attach-profile',password:'Attach-Profile-Password!',email:'attach@example.test'},q,true) as {id:string};
  await rows(`DELETE FROM "EmployeeProfiles" WHERE "IdentityUserId"=$1`,[identity.id]);
  const attached=await users(admin,identity.id,'profile','POST',undefined,q,true) as {IdentityUserId:string};assert.equal(attached.IdentityUserId,identity.id);
  await assert.rejects(()=>users(admin,identity.id,'profile','POST',undefined,q,true),/уже существует/);
});
test('package assignment semantics remain independent of public active contract eligibility',{skip:!enabled},async()=>{
  const pkg=await transaction(db=>insert('ServicePackages',{Name:'Package test',Style:''},db,admin.userId));
  await contracts(admin,contractId,'replace-package','POST',{servicePackageId:pkg.Id},q);
  const assigned=await customers(admin,customerId,'packages','GET',undefined,q) as {items:{Id:string}[]};assert.ok(assigned.items.some(p=>p.Id===pkg.Id));
  const unused=await customers(admin,customerId,'packages','GET',undefined,new URLSearchParams({assigned:'false'})) as {items:{Id:string}[]};assert.equal(unused.items.some(p=>p.Id===pkg.Id),false);
  const st=await one(`SELECT "StatusId" FROM "Contracts" WHERE "Id"=$1`,[contractId]);
  const future=await contracts(admin,undefined,undefined,'POST',{name:'Future public eligibility',tenantId:customerId,startsAt:'2090-01-01T00:00:00Z',endsAt:'2099-12-31T00:00:00Z',statusId:st!.StatusId},q) as {Id:string};
  const active=await contracts(admin,undefined,undefined,'GET',undefined,new URLSearchParams({active:'true',tenantId:customerId})) as {items:{Id:string}[]};assert.equal(active.items.some(c=>c.Id===future.Id),false);
  const expiry=await contracts(admin,undefined,undefined,'GET',undefined,new URLSearchParams({active:'true',activePolicy:'expiry',tenantId:customerId})) as {items:{Id:string}[]};assert.ok(expiry.items.some(c=>c.Id===future.Id));
});
test('tag permissions match separate legacy read, create and delete grants',{skip:!enabled},async()=>{
  const reader:Session={...admin,roles:[],permissions:['Ticket','Ticket.Get']};
  await tags(reader,'tickets',ticketId,'GET',undefined);
  await assert.rejects(()=>tags(reader,'tickets',ticketId,'POST',{name:'Permission tag'}),/Недостаточно/);
  await tags({...reader,permissions:[...reader.permissions,'Ticket.Create']},'tickets',ticketId,'POST',{name:'Permission tag'});
  await assert.rejects(()=>tags(reader,'tickets',ticketId,'DELETE',{name:'Permission tag'}),/Недостаточно/);
  await tags({...reader,permissions:[...reader.permissions,'Ticket.Delete']},'tickets',ticketId,'DELETE',{name:'Permission tag'});
});
test('notification status counts are personal and repeated reads preserve their timestamp',{skip:!enabled},async()=>{
  const before=await notifications(client,undefined,'statuses','GET',q) as {Status:number;Count:number}[];
  const notification=await transaction(db=>insert('UserNotifications',{IdentityUserId:client.userId,Title:'Status test',Url:'/'},db,admin.userId));
  const current=await notifications(client,undefined,'statuses','GET',q) as {Status:number;Count:number}[];
  const unread=(items:{Status:number;Count:number}[])=>items.find(s=>s.Status===0)?.Count??0;
  if(current.some(s=>s.Status===0))assert.equal(unread(current),unread(before)+1);
  await notifications(client,String(notification.Id),'read','POST',q);const read=await one(`SELECT "Seen" FROM "UserNotifications" WHERE "Id"=$1`,[notification.Id]);
  await notifications(client,String(notification.Id),'read','POST',q);assert.equal(String((await one(`SELECT "Seen" FROM "UserNotifications" WHERE "Id"=$1`,[notification.Id]))!.Seen),String(read!.Seen));
});
test('monthly counters handle past years, moved groups, multiple settings and deleted history',{skip:!enabled},async()=>{
  const types=await rows(`SELECT "Id" FROM "TicketTypes" LIMIT 2`),sections=await rows(`SELECT "Id" FROM "TicketSections" LIMIT 2`),constraint=await one(`SELECT "Id" FROM "ConstraintTypes" LIMIT 1`);
  const ticket=await createTicket(client,{subject:'Monthly boundaries',description:'Test',ticketTypeId:types[0].Id,ticketSectionId:sections[0].Id,contractId});
  await transaction(async db=>{
    await db.query(`UPDATE "Tickets" SET "CreationTime"='2025-12-31 12:00:00' WHERE "Id"=$1`,[ticket.Id]);
    await insert('ContractSettings',{ContractId:contractId,ConstraintTypeId:constraint!.Id,Max:11},db,admin.userId);
    const history=await insert('TicketHistory',{TicketId:ticket.Id,TicketStatusId:ticket.TicketStatusId,Description:'Historical spend',Type:1,IsInternal:false},db,admin.userId);
    await insert('TicketHoursSpentHistories',{TicketHistoryId:history.Id,ConstraintTypeId:constraint!.Id,Spent:4,CreationTime:new Date('2025-12-31T12:00:00Z')},db,admin.userId);
    assert.equal((await one(`SELECT "Sum" FROM "ContractConstraintsByMonths" WHERE "ContractId"=$1 AND "ConstraintTypeId"=$2 AND "Year"=2025 AND "Month"=12`,[contractId,constraint!.Id],db))!.Sum,4);
    await update('TicketHistory',history.Id,{IsDeleted:true},db,admin.userId);
    assert.equal((await one(`SELECT "Sum" FROM "ContractConstraintsByMonths" WHERE "ContractId"=$1 AND "ConstraintTypeId"=$2 AND "Year"=2025 AND "Month"=12`,[contractId,constraint!.Id],db))!.Sum,0);
  });
  await rows(`UPDATE "Tickets" SET "TicketTypeId"=$2,"TicketSectionId"=$3,"CreationTime"='2026-01-01 12:00:00' WHERE "Id"=$1`,[ticket.Id,types[1].Id,sections[1].Id]);
  assert.equal((await one(`SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1 AND "TicketTypeId"=$2 AND "Year"=2025 AND "Month"=12`,[contractId,types[0].Id]))!.Count,0);
  assert.equal((await one(`SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1 AND "TicketTypeId"=$2 AND "Year"=2026 AND "Month"=1`,[contractId,types[1].Id]))!.Count,1);
  await rows(`UPDATE "Tickets" SET "IsDeleted"=true WHERE "Id"=$1`,[ticket.Id]);
  assert.equal((await one(`SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1 AND "TicketTypeId"=$2 AND "Year"=2026 AND "Month"=1`,[contractId,types[1].Id]))!.Count,0);
});
test('serialization retry rolls back failed attempt before executing the command again',{skip:!enabled},async()=>{
  let attempts=0;const name='Serialization test '+randomUUID();
  await transaction(async db=>{await insert('Regions',{Name:name},db,admin.userId);attempts++;if(attempts===1)throw Object.assign(new Error('serialization fixture'),{code:'40001'});});
  assert.equal(attempts,2);assert.equal((await rows(`SELECT "Id" FROM "Regions" WHERE "Name"=$1`,[name])).length,1);
});

test('concurrent contract updates serialize aggregate refresh without losing ticket counts',{skip:!enabled},async()=>{
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`);
  const ids=await Promise.all(Array.from({length:6},(_,i)=>createTicket(admin,{subject:`Concurrent ${i}`,description:'Test',ticketTypeId:type!.Id,contractId})));
  assert.equal(new Set(ids.map(t=>t.Id)).size,6);
  const actual=await one(`SELECT count(*)::integer AS n FROM "Tickets" WHERE "ContractId"=$1 AND "TicketTypeId"=$2 AND NOT "IsDeleted" AND extract(year FROM "CreationTime")=extract(year FROM now()) AND extract(month FROM "CreationTime")=extract(month FROM now())`,[contractId,type!.Id]);
  const aggregate=await one(`SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1 AND "TicketTypeId"=$2 AND "Year"=extract(year FROM now()) AND "Month"=extract(month FROM now())`,[contractId,type!.Id]);
  assert.equal(aggregate!.Count,actual!.n);
});

test('explicit open calendar validates full years, caches working dates and rejects malformed responses',{skip:!enabled},async()=>{
  const previous=process.env.WORK_CALENDAR_PROVIDER,original=globalThis.fetch;
  process.env.WORK_CALENDAR_PROVIDER='isdayoff';let calls=0;
  globalThis.fetch=async()=>{calls++;return new Response('110'+'0'.repeat(362));};
  try{
    const result=await productionCalendar('2031-01-01','2031-01-03');assert.equal(result.countDays,3);assert.equal(result.countWorkdays,1);assert.deepEqual(result.holidays,['2031-01-01','2031-01-02']);
    await productionCalendar('2031-02-01','2031-02-02');assert.equal(calls,1);
    globalThis.fetch=async()=>new Response('100');await assert.rejects(()=>productionCalendar('2032-01-01','2032-01-03'),/Некорректный ответ/);
  }finally{globalThis.fetch=original;if(previous===undefined)delete process.env.WORK_CALENDAR_PROVIDER;else process.env.WORK_CALENDAR_PROVIDER=previous;}
});
test('email recovery tokens are single-use, tenant-scoped and revoke existing sessions; SMTP sends through a real protocol connection',{skip:!enabled},async()=>{
  const delivered:string[]=[];
  const smtp=createTcpServer(socket=>{
    socket.write('220 localhost ESMTP\r\n');let buffer='',data=false,message='';
    socket.on('data',chunk=>{buffer+=chunk.toString();while(buffer.includes('\r\n')){const at=buffer.indexOf('\r\n'),line=buffer.slice(0,at);buffer=buffer.slice(at+2);
      if(data){if(line==='.'){delivered.push(message);message='';data=false;socket.write('250 queued\r\n');}else message+=line+'\n';}
      else if(line.startsWith('EHLO')||line.startsWith('HELO'))socket.write('250-localhost\r\n250 8BITMIME\r\n');
      else if(line==='DATA'){data=true;socket.write('354 End with dot\r\n');}
      else if(line==='QUIT'){socket.end('221 bye\r\n');}else socket.write('250 OK\r\n');
    }});
  });
  await new Promise<void>(resolve=>smtp.listen(0,'127.0.0.1',resolve));
  const previous={...process.env};Object.assign(process.env,{SMTP_HOST:'127.0.0.1',SMTP_PORT:String((smtp.address() as {port:number}).port),SMTP_FROM:'noreply@example.test',SMTP_REQUIRE_TLS:'false',APP_URL:'https://showcase.example.test'});delete process.env.SMTP_USER;
  try{
    const user=await users(admin,undefined,undefined,'POST',{userName:'recover-user',password:'Recovery-Original-Password!',email:'recover@example.test',tenantId:customerId},q) as {id:string};
    await login('recover-user','Recovery-Original-Password!','Тест','recovery-test');
    const known=await requestPasswordReset({userName:'recover-user',tenant:'Тест'}),unknown=await requestPasswordReset({userName:'absent-user',tenant:'Тест'});assert.deepEqual(known,unknown);
    const mail=await one(`SELECT "Payload" FROM "NextJobs" WHERE "Kind"='email.send' AND "Payload"->>'to'='recover@example.test' ORDER BY "CreatedAt" DESC LIMIT 1`);
    const payload=mail!.Payload as {to:string;subject:string;text:string};await sendEmail(payload);assert.equal(delivered.length,1);
    const url=new URL(payload.text.match(/https:\/\/\S+/)![0]),token=new URLSearchParams(url.hash.slice(1)).get('token')!;assert.match(token,/^[a-f0-9]{64}$/);
    assert.notEqual((await one(`SELECT "TokenHash" FROM "NextRecoveryTokens" WHERE "UserId"=$1`,[user.id]))!.TokenHash,token);
    await resetPassword({token,newPassword:'Recovery-New-Password!'});
    assert.equal((await rows(`SELECT "UserId" FROM "NextSessions" WHERE "UserId"=$1`,[user.id])).length,0);
    await assert.rejects(()=>resetPassword({token,newPassword:'Replay-Password!'}),/недействительна/);
    await assert.rejects(()=>login('recover-user','Recovery-Original-Password!','Тест','recovery-test'),/Неверные/);
    await login('recover-user','Recovery-New-Password!','Тест','recovery-test');
    await rows(`UPDATE "AbpUsers" SET "EmailConfirmed"=true,"TwoFactorEnabled"=true WHERE "Id"=$1`,[user.id]);
    await assert.rejects(()=>login('recover-user','Recovery-New-Password!','Тест','mfa-test'),/Код входа отправлен/);
    const otpMail=await one(`SELECT "Payload" FROM "NextJobs" WHERE "Kind"='email.send' AND "Payload"->>'to'='recover@example.test' ORDER BY "CreatedAt" DESC LIMIT 1`);
    const code=(otpMail!.Payload as {text:string}).text.match(/\b\d{6}\b/)![0];
    await assert.rejects(()=>login('recover-user','Recovery-New-Password!','Тест','mfa-test','000000'),/код/);
    await login('recover-user','Recovery-New-Password!','Тест','mfa-test',code);
    await assert.rejects(()=>login('recover-user','Recovery-New-Password!','Тест','mfa-test',code),/код/);
  }finally{
    for(const key of ['SMTP_HOST','SMTP_PORT','SMTP_FROM','SMTP_REQUIRE_TLS','SMTP_USER','APP_URL']){if(previous[key]===undefined)delete process.env[key];else process.env[key]=previous[key];}
    await new Promise<void>(resolve=>smtp.close(()=>resolve()));
  }
});
test('staff create on behalf of a customer validates organization and preserves the chosen author',{skip:!enabled},async()=>{
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`),status=await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"='InProgress'`);
  const ticket=await createTicket(admin,{subject:'Created on behalf',description:'Test',ticketTypeId:type!.Id,contractId,onBehalfOfUserId:client.userId,statusId:status!.Id});
  assert.equal(ticket.CreatorId,client.userId);assert.equal(ticket.TenantId,customerId);assert.equal(ticket.TicketStatusId,status!.Id);
  await assert.rejects(()=>createTicket(client,{subject:'forged',description:'Test',ticketTypeId:type!.Id,contractId,onBehalfOfUserId:admin.userId}),/сотрудникам/);
  await assert.rejects(()=>createTicket(admin,{subject:'invalid',description:'Test',ticketTypeId:type!.Id,contractId:randomUUID()}),/не найден/);
});
test('legacy separate messages create a recipient notification and staff calendar includes their customer-ticket events',{skip:!enabled},async()=>{
  const before=Number((await one(`SELECT count(*)::integer AS n FROM "UserNotifications" WHERE "IdentityUserId"=$1`,[client.userId]))!.n);
  const message=await messages(admin,ticketId,'POST',{comment:'Separate conversation'},q) as {Comment:string};assert.equal(message.Comment,'Separate conversation');
  assert.equal(Number((await one(`SELECT count(*)::integer AS n FROM "UserNotifications" WHERE "IdentityUserId"=$1`,[client.userId]))!.n),before+1);
  const page=await messages(admin,ticketId,'GET',undefined,q) as {items:{Comment:string}[]};assert.ok(page.items.some(m=>m.Comment==='Separate conversation'));
  const date=new Date(),event=await events(admin,undefined,'POST',{title:'Staff cross-tenant own event',date:date.toISOString(),ticketId},q) as {Id:string};
  const own=await events(admin,undefined,'GET',undefined,new URLSearchParams({mine:'true',start:new Date(+date-60000).toISOString(),end:new Date(+date+60000).toISOString()})) as {Id:string}[];
  assert.ok(own.some(e=>e.Id===event.Id));await events(admin,event.Id,'PATCH',{title:'Updated staff event'},q);
});
test('XLSX exports retain the legacy ticket columns and all three statistics groups',{skip:!enabled},async()=>{
  const tickets=await spreadsheet(admin,'tickets',q),book=new ExcelJS.Workbook();await book.xlsx.read(Readable.from([Buffer.from(await tickets.arrayBuffer())]));
  assert.deepEqual((book.worksheets[0].getRow(1).values as unknown[]).slice(1),['Номер заявки','Наименование клиента','Тэг','Тип','Раздел','Специалист','Дата создания']);
  const stats=await spreadsheet(admin,'statistics',new URLSearchParams({customerId})),second=new ExcelJS.Workbook();await second.xlsx.read(Readable.from([Buffer.from(await stats.arrayBuffer())]));
  assert.equal(second.worksheets[0].getRow(1).getCell(1).value,'Раздел Название');assert.equal(second.worksheets[0].getRow(1).getCell(3).value,'Тип Название');assert.equal(second.worksheets[0].getRow(1).getCell(5).value,'Ограничение Название');
});

test('ticket spend totals use the matching constraint and never reveal internal or foreign history',{skip:!enabled},async()=>{
  const type=await one(`SELECT "Id" FROM "TicketTypes" LIMIT 1`),constraint=await one(`SELECT "Id","Name" FROM "ConstraintTypes" LIMIT 1`);
  const ticket=await createTicket(client,{subject:'Spend summary',description:'Test',ticketTypeId:type!.Id,contractId});
  const visible=await mutateTicket(admin,String(ticket.Id),'reply',{description:'Visible response'}),hidden=await mutateTicket(admin,String(ticket.Id),'reply',{description:'Internal response',internal:true});
  const result=await spentTime(admin,String(ticket.Id),{historyId:visible.Id,constraintTypeId:constraint!.Id,spent:3});assert.equal(result.ConstraintTypeName,constraint!.Name);
  await spentTime(admin,String(ticket.Id),{historyId:hidden.Id,constraintTypeId:constraint!.Id,spent:9});
  assert.equal((await ticketSpent(client,String(ticket.Id)))[0].NowCount,3);assert.equal((await ticketSpent(admin,String(ticket.Id)))[0].NowCount,12);
  await assert.rejects(()=>ticketSpent(other,String(ticket.Id)),/не найдена/);
});

test('content adapters preserve legacy DTO fields, cover images and provider pagination without exposing keys',{skip:!enabled},async()=>{
  const original=globalThis.fetch,previous={...process.env};
  Object.assign(process.env,{CONTENT_API_URL:'https://content.example.test/api/',CONTENT_API_AUTHORIZATION:'Bearer fixture-only',BK_NEWS_KEY:'fixture-only'});
  try{
    globalThis.fetch=async input=>{const url=new URL(String(input));assert.equal(url.searchParams.get('pagination[page]'),'2');return Response.json({data:[{id:5,attributes:{title:'Article',text:'Body',slug:'article',createdAt:'2026-01-01',seo:[]}}],meta:{pagination:{total:12}}});};
    const articles=await content('articles',new URLSearchParams({page:'2',take:'5'}));assert.equal(articles.totalCount,12);assert.equal(articles.items[0].Title,'Article');assert.equal(articles.data[0].attributes.title,'Article');assert.equal(articles.items[0].CreationTime,'2026-01-01');
    globalThis.fetch=async input=>{const url=new URL(String(input));assert.equal(url.searchParams.get('page'),'3');return Response.json({total:51,results:[{title:'News',_url:'/news/1',publishedAt:'2026-01-01',_purpose:[{title:'Accounting'}],thumbnail:{items:[{_pieces:[{item:{attachment:{_urls:{full:'https://images.example.test/news.jpg'}}}}]}]}}]});};
    const news=await legacyContent('news',new URLSearchParams({take:'20',skip:'40'}));assert.equal(news.items[0].CoverUrls,'https://images.example.test/news.jpg');assert.deepEqual(news.items[0].Purpose,['Accounting']);assert.equal(news.totalCount,51);
  }finally{globalThis.fetch=original;for(const key of ['CONTENT_API_URL','CONTENT_API_AUTHORIZATION','BK_NEWS_KEY']){if(previous[key]===undefined)delete process.env[key];else process.env[key]=previous[key];}}
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
