import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { loadModel } from './helpers/load-model.mjs';
const model=loadModel(fileURLToPath(new URL('../src/entities/ticket/model/detail.ts',import.meta.url)));
const {tickets}=loadModel(fileURLToPath(new URL('../src/entities/ticket/model/mock.ts',import.meta.url)));
const detail=model.createTicketDetail(tickets[0]);
const now='2026-10-06T03:00:00Z';
const input={text:'Ответ клиенту',internal:false,status:'В работе',constraint:'НПА',units:1,attachments:[]};

test('Sending a client reply preserves history, appends status change and charges the selected constraint',()=>{
 const next=model.sendMessage(detail,{...input,status:'Закрыта'},'reply',now);
 assert.equal(next.ticket.status,'Закрыта');assert.equal(next.messages.at(-2).kind,'reply');assert.equal(next.messages.at(-1).kind,'event');
 assert.equal(next.constraints.find(c=>c.name==='НПА').used,4);assert.equal(detail.constraints.find(c=>c.name==='НПА').used,3);
 assert.equal(next.updatedAt,now);assert.equal(next.messages.length,detail.messages.length+2);
});
test('Internal notes cannot close the ticket or consume client allowance',()=>{
 const next=model.sendMessage(detail,{...input,internal:true,status:'Закрыта',units:99},'internal',now);
 assert.equal(next.ticket.status,detail.ticket.status);assert.deepEqual(next.constraints,detail.constraints);assert.equal(next.messages.at(-1).kind,'internal');
});
test('Empty answers and attachment-only closure are rejected',()=>{
 assert.throws(()=>model.sendMessage(detail,{...input,text:'  ',status:'Закрыта',attachments:[{id:'a',name:'a.txt',size:1}]},'empty',now),/закрытия/);
 assert.throws(()=>model.sendMessage(detail,{...input,text:'\n',internal:true},'empty',now),/сообщение/);
});
test('Allowance checks reject excessive, fractional, negative and unknown deductions',()=>{
 for(const units of [8,-1,1.5])assert.throws(()=>model.sendMessage(detail,{...input,units},'bad',now));
 assert.throws(()=>model.sendMessage(detail,{...input,constraint:'unknown'},'bad',now),/лимит/);
 const next=model.sendMessage(detail,{...input,units:7},'exact',now);assert.equal(next.constraints[0].used,10);
});
test('The legacy attachment limit is 50 MB across the entire selection',()=>{
 assert.equal(model.validateAttachments([{size:30*1024*1024},{size:20*1024*1024}]),null);
 assert.ok(model.validateAttachments([{size:30*1024*1024},{size:20*1024*1024+1}]));
});
test('Changing assignment adds a system event and keeps all messages',()=>{
 const next=model.updateTicketField(detail,{responsibleId:'morozov',responsible:'Морозов Иван'},'Заявка назначена на Морозов Иван','assign',now);
 assert.equal(next.ticket.responsibleId,'morozov');assert.equal(next.messages.at(-1).kind,'event');assert.equal(detail.ticket.responsibleId,'smirnova');
});
test('Scheduled events validate both text and date',()=>{
 assert.throws(()=>model.addTicketEvent(detail,' ','2026-10-07T10:00','e',now));
 assert.throws(()=>model.addTicketEvent(detail,'Позвонить','bad','e',now));
 const next=model.addTicketEvent(detail,'Позвонить клиенту','2026-10-07T10:00','e',now);
 assert.equal(next.events.length,1);assert.equal(next.messages.at(-1).kind,'event');
});
test('Deadline progress is clamped, including deadlines preceding creation',()=>{
 assert.equal(model.deadlineProgress(tickets[0],Date.UTC(2020,0,1)),0);
 assert.equal(model.deadlineProgress(tickets[0],Date.UTC(2030,0,1)),100);
 assert.equal(model.deadlineProgress({...tickets[0],deadline:'01.01.2000'},Date.UTC(2026,9,6)),100);
});
test('Corrupt persisted state is rejected before rendering',()=>{
 assert.ok(model.isDetailMap({[tickets[0].id]:detail}));
 assert.ok(model.isDetailMap({}));
 assert.equal(model.isDetailMap({[tickets[0].id]:{...detail,messages:[{kind:'reply'}]}}),false);
 assert.equal(model.isDetailMap({[tickets[0].id]:{...detail,constraints:[{name:'НПА',used:-1,limit:5}]}}),false);
 assert.equal(model.isDetailMap({wrong:detail}),false);
 assert.equal(model.isDetailMap({[tickets[0].id]:{...detail,ticket:{...detail.ticket,status:'unknown'}}}),false);
});
test('Deleted tickets cannot accept new replies',()=>{
 assert.throws(()=>model.sendMessage({...detail,deleted:true},input,'deleted',now),/удалена/);
});
