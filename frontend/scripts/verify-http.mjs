import { spawn } from 'node:child_process';
import { createServer } from 'node:net';
import assert from 'node:assert/strict';
const database=process.env.BACKEND_TEST_DATABASE_URL;
assert.ok(database?.includes('/backend_test'),'Set BACKEND_TEST_DATABASE_URL to the isolated verification database');
const probe=createServer();await new Promise(resolve=>probe.listen(0,'127.0.0.1',resolve));const port=probe.address().port;await new Promise(resolve=>probe.close(resolve));
const origin=`http://127.0.0.1:${port}`;let log='';
const child=spawn(process.execPath,['node_modules/next/dist/bin/next','start','--hostname','127.0.0.1','--port',String(port)],{cwd:process.cwd(),windowsHide:true,env:{...process.env,DATABASE_URL:database,APP_URL:origin,NODE_ENV:'production'}});
child.stdout.on('data',data=>{log=(log+data).slice(-6000);});child.stderr.on('data',data=>{log=(log+data).slice(-6000);});
const request=(path,{method='GET',body,cookie,source=origin}={})=>fetch(origin+path,{method,headers:{...(cookie?{Cookie:cookie}:{}),...(method!=='GET'?{Origin:source}:{}),...(body!==undefined?{'Content-Type':'application/json'}:{})},body:body===undefined?undefined:JSON.stringify(body),redirect:'manual',signal:AbortSignal.timeout(15000)});
try{
  let ready=false;
  for(let attempt=0;attempt<120;attempt++){
    if(child.exitCode!==null)throw new Error('Next.js exited before startup: '+log);
    try{if((await request('/api/health')).ok){ready=true;break;}}catch{}
    await new Promise(resolve=>setTimeout(resolve,250));
  }
  assert.ok(ready,'Next.js did not start: '+log);
  assert.equal((await request('/api/v1/tickets')).status,401);
  assert.equal((await request('/api/auth/login',{method:'POST',source:'https://foreign.example',body:{userName:'integration-admin',password:'Integration-Admin-Password!'}})).status,403);
  const login=await request('/api/auth/login',{method:'POST',body:{userName:'integration-admin',password:'Integration-Admin-Password!'}});assert.equal(login.status,200,await login.text());
  const adminCookie=login.headers.get('set-cookie')?.split(';')[0];assert.ok(adminCookie);
  const session=await request('/api/auth/session',{cookie:adminCookie});assert.equal(session.status,200);assert.ok((await session.json()).roles.includes('admin'));
  const list=await request('/api/v1/tickets',{cookie:adminCookie});assert.equal(list.status,200);const data=await list.json();assert.ok(data.items.length);
  const id=String(data.items.find(t=>t.Subject==='Вопрос').Id);
  assert.equal((await request(`/api/v1/tickets/${id}/description`,{cookie:adminCookie,method:'POST',source:'https://foreign.example',body:{description:'forged'}})).status,403);
  assert.equal((await request(`/api/v1/tickets/${id}/description`,{cookie:adminCookie,method:'POST',body:{description:'forged',CreatorId:'spoof'}})).status,400);
  assert.equal((await request('/api/v1/tickets?sort=Id%3BDROP%20TABLE%20Tickets',{cookie:adminCookie})).status,400);
  const clientLogin=await request('/api/auth/login',{method:'POST',body:{userName:'client',password:'Client-Password!',tenant:'Тест'}});assert.equal(clientLogin.status,200,await clientLogin.text());
  const clientCookie=clientLogin.headers.get('set-cookie').split(';')[0];
  assert.equal((await request('/api/v1/users',{cookie:clientCookie})).status,403);
  const details=await request(`/api/v1/tickets/${id}`,{cookie:clientCookie});assert.equal(details.status,200);const ticket=await details.json();assert.ok(ticket.history.every(h=>!h.IsInternal && h.Type!==5));assert.equal(ticket.Description,'Описание');
  assert.equal((await request(`/api/v1/tickets/${id}/reply`,{cookie:clientCookie,method:'POST',body:{description:'forged',internal:true}})).status,403);
  assert.equal((await request(`/api/v1/tickets/${id}/read`,{cookie:clientCookie,method:'POST',body:{}})).status,200);
  const pdf=await request(`/api/v1/tickets/${id}/pdf`,{cookie:clientCookie});assert.equal(pdf.status,200);assert.equal(Buffer.from(await pdf.arrayBuffer()).subarray(0,4).toString(),'%PDF');
  assert.equal((await request('/api/auth/logout',{cookie:clientCookie,method:'POST'})).status,200);
  assert.equal((await request('/api/auth/session',{cookie:clientCookie})).status,401);
  assert.equal((await request('/api/auth/session',{cookie:adminCookie})).status,200);
  console.log('HTTP verification passed: login, sessions, CSRF, validation, tenant permissions, hidden history, PDF, logout');
}finally{
  if(child.exitCode===null){child.kill();await new Promise(resolve=>child.once('exit',resolve));}
}
