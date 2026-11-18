import { readFile } from 'node:fs/promises';
import assert from 'node:assert/strict';

const origin=process.env.SHOWCASE_URL;
assert.ok(origin?.startsWith('https://'),'Set SHOWCASE_URL to the HTTPS showcase origin');
const passwords=JSON.parse(await readFile(process.argv[2],'utf8'));
const cookies=new Map();
async function request(path,login,method='GET',body,source=origin){
  return fetch(origin+path,{method,redirect:'manual',signal:AbortSignal.timeout(30000),headers:{...(cookies.has(login)?{Cookie:cookies.get(login)}:{}),...(method!=='GET'?{Origin:source}:{}),...(body!==undefined&&!(body instanceof FormData)?{'Content-Type':'application/json'}:{})},body:body===undefined?undefined:body instanceof FormData?body:JSON.stringify(body)});
}
async function json(path,login,method='GET',body){const r=await request(path,login,method,body);assert.equal(r.status,200,`${method} ${path}: ${r.status} ${await r.clone().text()}`);return r.json();}
assert.equal((await request('/api/ready')).status,200);
assert.equal((await request('/api/v1/tickets')).status,401);
for(const login of Object.keys(passwords)){
  const r=await request('/api/auth/login',login,'POST',{userName:login,password:passwords[login],tenant:login==='demo.client'?'Альфа':login==='demo.other'?'Вектор':null});
  assert.equal(r.status,200,`Login ${login}: ${await r.clone().text()}`);const cookie=r.headers.get('set-cookie');assert.match(cookie,/HttpOnly/i);assert.match(cookie,/Secure/i);cookies.set(login,cookie.split(';')[0]);
  const me=await json('/api/auth/session',login);assert.equal(me.userName,login);
  const page=await request(login.startsWith('demo.c')||login==='demo.other'?'/client':'/tickets',login);assert.equal(page.status,200);
}
const admin='demo.admin',client='demo.client',specialist='demo.specialist';
const clientView=await json('/api/v1/portal',client),otherView=await json('/api/v1/portal','demo.other');
assert.ok(clientView.tickets.length>=12);assert.ok(otherView.tickets.length>=4);
assert.ok(clientView.tickets.every(t=>!otherView.tickets.some(o=>o.id===t.id)));
const id=clientView.tickets.find(t=>t.attachments.length).id;
assert.equal((await request('/api/v1/tickets/'+id,'demo.other')).status,404);
const staffDetails=await json('/api/v1/tickets/'+id,specialist),clientDetails=await json('/api/v1/tickets/'+id,client);
assert.ok(staffDetails.history.some(h=>h.IsInternal));assert.ok(clientDetails.history.every(h=>!h.IsInternal&&h.Type!==5));
assert.equal((await request('/api/v1/users',client)).status,403);
assert.equal((await request('/api/v1/roles',specialist)).status,403);
assert.equal((await request('/api/v1/tickets/'+id+'/description',admin,'POST',{description:'forged'},'https://foreign.example')).status,403);
const file=clientDetails.attachments[0];const download=await request(`/api/v1/tickets/${id}/files/${file.Id}`,client);assert.equal(download.status,200);assert.ok((await download.text()).includes('Демонстрационное'));
for(const [path,signature] of [[`tickets/${id}/files/zip`,'PK'],[`tickets/${id}/pdf`,'%PDF'],['reports/tickets','PK']]){const r=await request('/api/v1/'+path,admin);assert.equal(r.status,200,path);assert.ok(Buffer.from(await r.arrayBuffer()).subarray(0,8).toString().startsWith(signature));}
const types=await json('/api/v1/catalogs/ticket-types',client),statuses=await json('/api/v1/catalogs/ticket-statuses',admin);
const form=new FormData();form.set('data',JSON.stringify({subject:'ДЕМО: сквозная проверка '+new Date().toISOString(),description:'Проверка создания заявки с настоящим S3-вложением.',ticketTypeId:types.items[0].Id,contractId:clientView.contracts[0].id}));form.append('files',new File(['Демонстрационное вложение сквозной проверки'],'Проверка.txt',{type:'text/plain'}));
const ticket=await json('/api/v1/tickets',client,'POST',form),newId=String(ticket.Id);
await json(`/api/v1/tickets/${newId}/reply`,specialist,'POST',{description:'Консультация подготовлена на демонстрационном стенде.',statusId:statuses.items.find(s=>s.Name==='Answered').Id});
await json(`/api/v1/tickets/${newId}/reply`,client,'POST',{description:'Просим уточнить рекомендацию.'});
assert.equal((await json('/api/v1/tickets/'+newId,client)).status.Name,'Renewed');
await json(`/api/v1/tickets/${newId}/favorite`,client,'POST',{favorite:true});
assert.ok((await json('/api/v1/notifications',admin)).items.length);
await json('/api/auth/logout',client,'POST');assert.equal((await request('/api/auth/session',client)).status,401);
console.log('SHOWCASE PASSED: HTTPS, six accounts, role permissions, tenant isolation, hidden messages, real S3 download/upload, ZIP, PDF, XLSX, multipart ticket, specialist reply, reopen, favorite, notifications, logout.');
