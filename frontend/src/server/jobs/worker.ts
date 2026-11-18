import { randomUUID } from 'node:crypto';
import { pool, one, rows, transaction } from '../db';
import { queueJob } from './queue';
import { assert } from '../errors';
import amqp from 'amqplib';
import { deleteObject, type Container } from '../storage/s3';
import { applyTenantEvent } from './tenant-events';
import { createHash } from 'node:crypto';
import { ZodError } from 'zod';
import { CronExpressionParser } from 'cron-parser';
import { sendEmail } from '../integrations/email';

let stopped=false;
for(const signal of ['SIGTERM','SIGINT'] as const)process.on(signal,()=>{stopped=true;});
async function publish(kind:string,payload:Record<string,unknown>,id:string) {
  assert(process.env.RABBITMQ_URL && process.env.RABBITMQ_EXCHANGE,503,'RabbitMQ не настроен');
  const routingKey=process.env[kind==='tenant.created'?'RABBITMQ_TENANT_CREATED_KEY':'RABBITMQ_TENANT_DELETED_KEY'];
  assert(routingKey,503,'Не настроен routing key старого события');
  const connection=await amqp.connect(process.env.RABBITMQ_URL);
  try {
    const channel=await connection.createConfirmChannel();
    try {
      await channel.assertExchange(process.env.RABBITMQ_EXCHANGE,'direct',{durable:true});
      let unrouted=false;channel.once('return',()=>{unrouted=true;});
      channel.publish(process.env.RABBITMQ_EXCHANGE,routingKey,Buffer.from(JSON.stringify({Entity:{Id:payload.id,...(payload.name?{Name:payload.name}:{})}})),{persistent:true,messageId:id,contentType:'application/json',mandatory:true});
      await channel.waitForConfirms();
      assert(!unrouted,503,'Событие не привязано ни к одной очереди');
    }finally{await channel.close();}
  }finally{await connection.close();}
}
let inbox:Awaited<ReturnType<Awaited<ReturnType<typeof amqp.connect>>['createChannel']>>|null=null;
let inboxConnection:Awaited<ReturnType<typeof amqp.connect>>|null=null;
let nextInboxAttempt=0;
async function receiveTenantEvent() {
  const queue=process.env.RABBITMQ_CONSUMER_QUEUE,url=process.env.RABBITMQ_URL,exchange=process.env.RABBITMQ_EXCHANGE,created=process.env.RABBITMQ_TENANT_CREATED_KEY,deleted=process.env.RABBITMQ_TENANT_DELETED_KEY;
  if(!queue||!url||!exchange||!created||!deleted||nextInboxAttempt>Date.now())return;
  try {
    if(!inbox){
      inboxConnection=await amqp.connect(url);inboxConnection.on('error',()=>{inbox=null;nextInboxAttempt=Date.now()+30000;});inboxConnection.on('close',()=>{inbox=null;});
      inbox=await inboxConnection.createChannel();inbox.on('error',()=>{inbox=null;nextInboxAttempt=Date.now()+30000;});
      await inbox.assertExchange(exchange,'direct',{durable:true});await inbox.assertQueue(queue,{durable:true});await inbox.bindQueue(queue,exchange,created);await inbox.bindQueue(queue,exchange,deleted);
    }
    const channel=inbox,message=await channel.get(queue,{noAck:false});if(!message)return;
    const id=message.properties.messageId??createHash('sha256').update(message.fields.routingKey).update(message.content).digest('hex');
    if(message.fields.routingKey!==created && message.fields.routingKey!==deleted){await queueJob('tenant.unhandled',{routingKey:message.fields.routingKey,body:message.content.toString('base64')},'tenant.unhandled:'+id);channel.ack(message);return;}
    try {
      await applyTenantEvent(message.fields.routingKey===created?'created':'deleted',JSON.parse(message.content.toString('utf8')),id);channel.ack(message);
    }catch(error){
      if(error instanceof SyntaxError||error instanceof ZodError){
        // Preserve malformed messages durably for operator review instead of dropping them.
        await queueJob('tenant.invalid',{routingKey:message.fields.routingKey,body:message.content.toString('base64')},'tenant.invalid:'+id);
        channel.ack(message);
      }else{channel.nack(message,false,true);throw error;}
    }
  }catch(error){
    console.error('Tenant event receiver failed',{name:(error as Error).name});nextInboxAttempt=Date.now()+30000;inbox=null;
    if(inboxConnection)try{await inboxConnection.close();}catch{}inboxConnection=null;
  }
}
export async function runJob() {
  const job=await transaction(db=>one(`UPDATE "NextJobs" SET "LockedUntil"=now()+interval '5 minutes',"Attempts"="Attempts"+1 WHERE "Id"=(SELECT "Id" FROM "NextJobs" WHERE "CompletedAt" IS NULL AND "AvailableAt"<=now() AND ("LockedUntil" IS NULL OR "LockedUntil"<now()) AND "Attempts"<10 ORDER BY "AvailableAt","CreatedAt" FOR UPDATE SKIP LOCKED LIMIT 1) RETURNING *`,[],db));
  if(!job)return false;
  try {
    if(job.Kind==='customers.refresh-active') {
      await rows(`UPDATE "TenantProfiles" p SET "IsActive"=EXISTS(SELECT 1 FROM "Contracts" c WHERE c."TenantId"=p."Id" AND NOT c."IsDeleted" AND c."ContractFinishDate">=now()),"LastModificationTime"=now() WHERE NOT p."IsDeleted"`);
    }else if(job.Kind==='file.delete-if-unreferenced') {
      const data=job.Payload as {id:string;tenantId:string|null;container?:Container};
      assert(!data.container||['attachments','public','employee-avatar'].includes(data.container),400,'Неизвестный контейнер');
      const referenced=await one(`SELECT 1 WHERE EXISTS(SELECT 1 FROM "TicketAttachments" WHERE "UploadedFileId"=$1) OR EXISTS(SELECT 1 FROM "TicketHistoryAttachments" WHERE "UploadedFileId"=$1) OR EXISTS(SELECT 1 FROM "CustomerUserProfiles" WHERE "UserAvatarFileId"=$1) OR EXISTS(SELECT 1 FROM "EmployeeProfiles" WHERE "UserAvatarFileId"=$1)`,[data.id]);
      if(!referenced)await deleteObject(data.container??'attachments',data.tenantId,data.id);
    }else if(job.Kind==='email.send')await sendEmail(job.Payload);
    else if(job.Kind==='story.delete-image'){
      const data=job.Payload as {id:string};
      assert(/^[0-9a-f-]{36}$/i.test(data.id),400,'Некорректный идентификатор изображения');
      const referenced=await one(`SELECT 1 WHERE EXISTS(SELECT 1 FROM "Stories" WHERE "Id"=$1::uuid OR "CoverImageUrl"=$1::text OR split_part("CoverImageUrl",'?',1) LIKE $2) OR EXISTS(SELECT 1 FROM "Slides" WHERE "Id"=$1::uuid OR "ImageUrl"=$1::text OR split_part("ImageUrl",'?',1) LIKE $2)`,[data.id,'%/'+data.id]);
      if(!referenced)await deleteObject('stories',null,data.id);
    }else if(job.Kind==='tenant.created'||job.Kind==='tenant.deleted')await publish(String(job.Kind),job.Payload as Record<string,unknown>,String(job.Id));
    else throw new Error('Unknown job kind');
    await rows(`UPDATE "NextJobs" SET "CompletedAt"=now(),"LockedUntil"=NULL,"LastError"=NULL WHERE "Id"=$1`,[job.Id]);
  }catch(error){
    console.error('Background job failed',{id:job.Id,kind:job.Kind,name:(error as Error).name});
    await rows(`UPDATE "NextJobs" SET "LockedUntil"=NULL,"LastError"=$2,"AvailableAt"=now()+make_interval(secs=>least(3600,power(2,"Attempts")::integer*10)) WHERE "Id"=$1`,[job.Id,(error as Error).message.slice(0,1000)]);
  }
  return true;
}
export async function worker() {
  console.log('Consulting worker started',{instance:randomUUID()});
  const timeZone=process.env.BUSINESS_TIMEZONE??'Europe/Moscow';
  const schedule=CronExpressionParser.parse(process.env.UPDATE_CUSTOMER_STATUS_CRON??'0 0 * * *',{tz:timeZone});
  let nextRefresh=schedule.next().toDate(),startupScheduled=false,maintenanceAt=0;
  while(!stopped) {
    try {
      if(!startupScheduled){const day=new Date().toLocaleDateString('en-CA',{timeZone});await queueJob('customers.refresh-active',{},'customers.refresh-active:startup:'+day);startupScheduled=true;}
      while(nextRefresh.getTime()<=Date.now()){await queueJob('customers.refresh-active',{},'customers.refresh-active:cron:'+nextRefresh.toISOString());nextRefresh=schedule.next().toDate();}
      if(Date.now()>=maintenanceAt){
        await rows(`DELETE FROM "NextSessions" WHERE "ExpiresAt"<now()`);
        await rows(`DELETE FROM "NextLoginAttempts" WHERE "WindowStart"<now()-interval '1 day'`);
        await rows(`DELETE FROM "NextCache" WHERE "ExpiresAt"<now()`);
        await rows(`DELETE FROM "NextRecoveryTokens" WHERE "ExpiresAt"<now()`);
        maintenanceAt=Date.now()+300_000;
      }
      await receiveTenantEvent();
      if(!await runJob())await new Promise(resolve=>setTimeout(resolve,2000));
    }catch(error){console.error('Worker cycle failed',{name:(error as Error).name});await new Promise(resolve=>setTimeout(resolve,5000));}
  }
  if(inboxConnection)await inboxConnection.close();
  await pool().end();
}
if(process.argv[1]?.endsWith('worker.ts'))void worker();
