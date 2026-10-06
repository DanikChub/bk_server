import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { loadModel } from './helpers/load-model.mjs';
const model=loadModel(fileURLToPath(new URL('../src/entities/notification/model/notifications.ts',import.meta.url)));
const {demoNotifications:rows}=loadModel(fileURLToPath(new URL('../src/entities/notification/model/mock.ts',import.meta.url)));
const user='demo-morozov', other='demo-makarovsky', now='2026-10-06T03:00:00Z';
const query=model.initialNotificationQuery;

test('Unread lists, counts and an empty identity never expose another recipient',()=>{
 assert.equal(model.selectNotifications(rows,user,query).length,2);
 assert.ok(model.selectNotifications(rows,user,{...query,status:'all'}).every(row=>row.identityUserId===user));
 assert.deepEqual(model.notificationCounts(rows,user),{all:12,unread:2,read:10});
 assert.deepEqual(model.selectNotifications(rows,'',{...query,status:'all'}),[]);
 assert.deepEqual(model.notificationCounts(rows,''),{all:0,unread:0,read:0});
});
test('Status, category and trimmed title filters combine; newest notifications come first',()=>{
 const found=model.selectNotifications(rows,user,{status:'all',categoryId:'tickets',title:'  СООБЩЕНИЕ  '});
 assert.equal(found.length,1);assert.equal(found[0].id,user+'-2');
 const all=model.selectNotifications(rows,user,{...query,status:'all'});
 for(let i=1;i<all.length;i++)assert.ok(Date.parse(all[i-1].createdAt)>=Date.parse(all[i].createdAt));
});
test('Marking selected notifications read changes the active unread list and counter immutably',()=>{
 const state=model.markNotificationsRead(rows,user,{},[user+'-1'],now);
 const next=model.notificationsForUser(rows,user,state);
 assert.equal(model.notificationCounts(next,user).unread,1);
 assert.deepEqual(model.selectNotifications(next,user,query).map(r=>r.id),[user+'-2']);
 assert.equal(rows.find(r=>r.id===user+'-1').seen,null);
});
test('Read updates reject foreign and unknown IDs and preserve original read timestamps',()=>{
 const state=model.markNotificationsRead(rows,user,{},[other+'-1','unknown',user+'-3'],now);
 assert.deepEqual(state,{});
 const first=model.markNotificationsRead(rows,user,{},[user+'-1'],now);
 assert.deepEqual(model.markNotificationsRead(rows,user,first,[user+'-1'],'2026-10-07T00:00:00Z'),first);
 assert.throws(()=>model.markNotificationsRead(rows,user,{},[user+'-1'],'invalid'));
});
test('Bulk read can clear the personal unread count without changing another account',()=>{
 const state=model.markNotificationsRead(rows,user,{},[user+'-1',user+'-2'],now);
 assert.equal(model.notificationCounts(model.notificationsForUser(rows,user,state),user).unread,0);
 assert.equal(model.notificationCounts(model.notificationsForUser(rows,other,state),other).unread,2);
});
test('Legacy card and message URLs preserve the employee view and only link existing tickets',()=>{
 for(const url of ['/tickets/details/128569','/tickets/detailsMessage/128569','/Tickets/Details/128569?Id=128569','/tickets/128569']) {
  assert.equal(model.notificationTicketHref(url,'/employee/tickets',[128569]),'/employee/tickets/128569');
  assert.equal(model.notificationTicketHref(url,'/tickets',[128569]),'/tickets/128569');
 }
 for(const url of [null,'/tickets/999','https://evil.test/tickets/128569','//evil.test/tickets/128569','javascript:alert(1)','/tickets/128569/../../employees','/tickets/128569\\anything','/tickets/0128569'])assert.equal(model.notificationTicketHref(url,'/tickets',[128569]),null);
});
test('Malformed read state is rejected before it can hide unread notifications',()=>{
 assert.equal(model.isNotificationReadState({[user+'-1']:now}),true);
 for(const state of [[],null,{a:true},{a:'not-a-date'},{a:null}])assert.equal(model.isNotificationReadState(state),false);
});
