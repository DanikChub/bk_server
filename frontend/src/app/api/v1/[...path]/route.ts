import { z } from 'zod';
import { api } from '@/server/http';
import { assert } from '@/server/errors';
import { listTickets, ticketDetails, createTicket, mutateTicket, deleteTicket, editHistory } from '@/server/modules/tickets';
import { contracts } from '@/server/modules/contracts';
import { customers } from '@/server/modules/customers';
import { users, profile, revealPassword, birthdays, changeOwnPassword } from '@/server/modules/users';
import { resource } from '@/server/modules/resources';
import { events } from '@/server/modules/events';
import { notifications } from '@/server/modules/notifications';
import { stories } from '@/server/modules/stories';
import { messages } from '@/server/modules/messages';
import { listFiles, uploadFiles, downloadFile, downloadZip, multipartForm } from '@/server/modules/files';
import { spreadsheet, ticketPdf } from '@/server/modules/reports';
import { productionCalendar } from '@/server/integrations/calendar';
import { content } from '@/server/integrations/content';
import { externalResource } from '@/server/integrations/external-auth';
import { requirePermission } from '@/server/auth/session';
import { portalSnapshot, portalProfile, completeEvent, renewal } from '@/server/modules/portal';
import { asset } from '@/server/modules/assets';
import { roles } from '@/server/modules/permissions';
import { tags } from '@/server/modules/tags';
import { rows, transaction } from '@/server/db';
import { requireStaff } from '@/server/auth/session';
import { legacyContent } from '@/server/integrations/legacy-content';
import { customerStatistics, workload, spentTime, readHistory, statusCounts } from '@/server/modules/statistics';
import { jobs } from '@/server/modules/jobs';
export const runtime='nodejs';
export const dynamic='force-dynamic';

const handler=api(async(req,s)=>{
  const parts=req.nextUrl.pathname.slice('/api/v1/'.length).split('/').map(decodeURIComponent);
  const [module,id,action,child]=parts,method=req.method,q=req.nextUrl.searchParams;
  assert(parts.length<=4,404,'Маршрут не найден');
  const permitted=(methods:string[])=>assert(methods.includes(method),405,'Метод не поддерживается');
  if(['contracts','customers','users','employees','roles'].includes(module)) {
    assert(!child,404,'Маршрут не найден');
    if(!id)permitted(['GET','POST']);
    else if(!action)permitted(['GET','PATCH','DELETE']);
    else {
      const operations:Record<string,Record<string,string[]>>={
        contracts:{statistics:['GET'],packages:['POST','DELETE'],limits:['POST'],tags:['GET','POST','DELETE']},
        customers:{statistics:['GET'],users:['GET'],events:['GET'],managers:['GET'],manager:['POST','DELETE'],contacts:['POST']},
        users:{password:['POST'],roles:['POST'],active:['POST'],'reveal-password':['POST']},employees:{password:['POST'],roles:['POST'],active:['POST'],'reveal-password':['POST']},roles:{permissions:['POST']},
      };
      const methods=operations[module]?.[action];assert(methods,404,'Операция не найдена');permitted(methods);
    }
  }
  if(module==='catalogs'){assert(id && !child,404,'Маршрут не найден');permitted(action?['GET','PATCH','DELETE']:['GET','POST']);}
  if(module==='notifications') {assert(!child,404,'Маршрут не найден');permitted(action==='read'?['POST']:['GET']);}
  if(module==='events'){assert(!action,404,'Маршрут не найден');permitted(id?['GET','PATCH','DELETE']:['GET','POST']);}
  if(module==='stories'){assert(!action||action==='slides',404,'Маршрут не найден');permitted(action==='slides'?(child?['PATCH','DELETE']:['POST']):id?['GET','PATCH','DELETE']:['GET','POST']);}
  if(module==='profile' && id==='avatar' && ['GET','POST'].includes(method))return asset(s,'avatar',method,req);
  if(module==='assets' && id==='stories' && ['GET','POST'].includes(method))return asset(s,'stories',method,req,action);
  const multipart=req.headers.get('content-type')?.startsWith('multipart/form-data');
  if(method==='POST' && multipart && !child && ((module==='tickets'&&(!id||action==='reply'||action==='draft'))||(module==='portal'&&id==='renewals'))){
    const form=await multipartForm(req),raw=form.get('data');assert(typeof raw==='string'&&raw.length<=2_000_000,400,'Нет данных операции');
    let data:unknown;try{data=JSON.parse(raw);}catch{assert(false,400,'Некорректный JSON');}
    return transaction(async()=>{
      const result=module==='portal'?await renewal(s,data):!id?await createTicket(s,data):await mutateTicket(s,id,action,data);
      const record=result as {Id:unknown},ticketId=module==='portal'||!id?String(record.Id):id;
      if(action==='reply')form.set('historyId',String(record.Id));
      if(form.getAll('files').length)await uploadFiles(s,ticketId,form);
      return result;
    });
  }
  if(module==='tickets' && action==='files') {
    if(child==='zip' && method==='GET')return downloadZip(s,id);
    if(child && method==='GET')return downloadFile(s,id,child);
    if(!child && method==='GET')return listFiles(s,id);
    if(!child && method==='POST')return uploadFiles(s,id,req);
    assert(false,405,'Метод не поддерживается');
  }
  let body:unknown;
  if(!['GET','HEAD','DELETE'].includes(method)) {
    assert(Number(req.headers.get('content-length')??0)<=2_000_000,413,'Слишком большой запрос');
    // Bound chunked bodies as well as requests with Content-Length.
    const reader=req.body?.getReader();let size=0;const chunks:Uint8Array[]=[];
    if(reader)try{for(;;){const part=await reader.read();if(part.done)break;size+=part.value.byteLength;assert(size<=2_000_000,413,'Слишком большой запрос');chunks.push(part.value);}}catch(e){await reader.cancel();throw e;}
    if(size)try{body=JSON.parse(Buffer.concat(chunks).toString('utf8'));}catch{assert(false,400,'Некорректный JSON');}
  } else if(method==='DELETE' && ['packages','manager','tags'].includes(action)) body=Object.fromEntries(q);
  if(module==='me' && method==='GET' && !id)return s;
  if(module==='profile' && id==='password' && !action && method==='POST')return changeOwnPassword(s,body);
  if(module==='jobs' && ((!id&&method==='GET')||(id&&action==='retry'&&method==='POST')))return jobs(s,id,method,q);
  if(module==='specialists' && id==='workload' && method==='GET')return workload(s,q);
  if(module==='specialists' && method==='GET' && !id){requireStaff(s);requirePermission(s,'SpecialistManagement.Get');return rows(`SELECT e."Id",e."FirstName",e."LastName",e."MiddleName",e."JobPost" FROM "EmployeeProfiles" e JOIN "AbpUsers" u ON u."Id"=e."IdentityUserId" WHERE u."IsActive" AND NOT u."IsDeleted" ORDER BY e."LastName"`);}
  if((module==='tickets'||module==='contracts') && id && action==='tags')return tags(s,module,id,method,body);
  if(module==='tickets') {
    if(id==='status-counts' && !action && method==='GET')return statusCounts(s);
    if(!id){assert(!action,404,'Маршрут не найден');if(method==='GET')return listTickets(s,q);if(method==='POST')return createTicket(s,body);}
    else if(!action){if(method==='GET')return ticketDetails(s,id);if(method==='DELETE')return deleteTicket(s,id);}
    else if(action==='messages' && ['GET','POST'].includes(method))return messages(s,id,method,body,q);
    else if(action==='history' && child && ['PATCH','DELETE'].includes(method))return editHistory(s,id,child,method,body);
    else if(action==='read-history' && child && method==='POST')return readHistory(s,id,child);
    else if(action==='spent' && method==='POST')return spentTime(s,id,body);
    else if(action==='pdf' && method==='GET')return ticketPdf(s,id);
    else if(!child && method==='POST')return mutateTicket(s,id,action,body);
    assert(false,405,'Метод не поддерживается');
  }
  if(module==='portal') {
    if(!id && method==='GET')return portalSnapshot(s);
    if(id==='profile' && method==='PATCH')return portalProfile(s,body);
    if(id==='renewals' && method==='POST')return renewal(s,body);
    if(id==='events' && action && method==='POST')return completeEvent(s,action,body);
    assert(false,405,'Метод не поддерживается');
  }
  if(module==='contracts')return contracts(s,id,action,method,body,q);
  if(module==='roles')return roles(s,id,action,method,body,q);
  if(module==='customers' && id && action==='statistics')return customerStatistics(s,id);
  if(module==='customers')return customers(s,id,action,method,body,q);
  if(module==='users' && id==='birthdays' && method==='GET')return birthdays(s,q);
  if((module==='users'||module==='employees') && id && action==='reveal-password')return revealPassword(s,id,module==='employees');
  if(module==='users'||module==='employees')return users(s,id,action,method,body,q,module==='employees');
  if(module==='profile' && !id && ['GET','PATCH'].includes(method))return profile(s,method,body);
  if(module==='catalogs' && id && !child)return resource(s,id,action,method,body,q);
  if(module==='events' && !action)return events(s,id,method,body,q);
  if(module==='notifications')return notifications(s,['categories','statuses','count'].includes(id)?undefined:id,action??(['categories','statuses','count'].includes(id)?id:undefined),method,q);
  if(module==='stories' && (!action || action==='slides'))return stories(s,id,action==='slides'?(child??'new'):undefined,method,body,q);
  if(module==='reports' && id && method==='GET')return spreadsheet(s,id,q);
  if(module==='calendar' && method==='GET') {requirePermission(s,'Calendar.Get');return productionCalendar(z.string().parse(q.get('start')),z.string().parse(q.get('end')));}
  if(module==='content' && id && method==='GET')return content(id,q);
  if(module==='legacy-content' && id && method==='GET')return legacyContent(id,q);
  if(module==='external-resource' && method==='GET')return Response.redirect(externalResource(s,z.string().url().parse(q.get('source')),z.string().parse(q.get('resourceId'))));
  assert(false,404,'Маршрут не найден');
});
export {handler as GET,handler as POST,handler as PATCH,handler as DELETE};
