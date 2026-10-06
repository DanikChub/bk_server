import assert from 'node:assert/strict';
import { test } from 'node:test';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

// Compile only the pure model modules; no browser, server or extra dependency needed.
function loadModel(file) {
  const unit = { exports: {} };
  const compiled = ts.transpileModule(fs.readFileSync(file, 'utf8'), { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2021 } }).outputText;
  new Function('require', 'module', 'exports', compiled)(relative => loadModel(path.resolve(path.dirname(file), relative + '.ts')), unit, unit.exports);
  return unit.exports;
}
const root = fileURLToPath(new URL('../src', import.meta.url));
const model = loadModel(path.join(root, 'widgets/tickets-list/model/query.ts'));
const fixtures = loadModel(path.join(root, 'entities/ticket/model/mock.ts'));
const { ticketsCsv } = loadModel(path.join(root, 'widgets/tickets-list/model/export.ts'));
const { tickets, specialists, demoSpecialistId, initialFavoriteTicketIds } = fixtures;
const select = (patch = {}) => model.selectTickets(tickets, { ...model.initialQuery, tab: 'all', ...patch }, demoSpecialistId, initialFavoriteTicketIds);

test('Mine is scoped to the current specialist; favorite results are separately scoped', () => {
  assert.ok(select({tab:'mine'}).every(ticket => ticket.responsibleId === demoSpecialistId));
  assert.equal(select({tab:'mine'}).length, tickets.filter(ticket => ticket.responsibleId === demoSpecialistId).length);
  assert.deepEqual(select({tab:'favorites'}).map(ticket => ticket.id).sort((a,b)=>a-b), [...initialFavoriteTicketIds].sort((a,b)=>a-b));
  assert.equal(model.selectTickets(tickets, {...model.initialQuery,tab:'favorites'},demoSpecialistId,[]).length,0);
});
test('Status tabs include exactly the expected tickets, including reopened tickets', () => {
  for (const [tab,status] of [['new','Новая'],['active','В работе'],['reopened','Возобновлённая'],['closed','Закрыта']]) {
    const rows = select({tab});assert.ok(rows.length);assert.ok(rows.every(ticket=>ticket.status===status));
    assert.equal(rows.length,tickets.filter(ticket=>ticket.status===status).length);
  }
});
test('General search matches exact IDs and normalized client names', () => {
  assert.deepEqual(select({search:'128 569'}).map(ticket=>ticket.id),[128569]);
  assert.ok(select({search:'  МИРНОВСКОГО  '}).length);
  assert.ok(select({search:'ЛИНЕВСКИЙ'}).length);
  assert.equal(select({search:'this does not exist'}).length,0);
});
test('Independent filters combine with status and specialist selection', () => {
  const filters={...model.emptyFilters,type:'консалтинг',tag:'срочно',createdAt:'2026-09-30'};
  assert.deepEqual(select({tab:'active',specialistId:'morozov',filters}).map(ticket=>ticket.id),[128569]);
  assert.equal(select({tab:'closed',specialistId:'morozov',filters}).length,0);
});
test('Pagination clamps a stale page after filtering or removing a favorite', () => {
  const rows=select();const p=model.paginateTickets(rows,2,10);assert.equal(p.rows.length,10);assert.equal(p.rows[0].id,rows[10].id);
  const last=model.paginateTickets(rows,999,10);assert.equal(last.currentPage,4);
  const empty=model.paginateTickets([],5,10);assert.deepEqual(empty,{rows:[],currentPage:1,pageCount:1});
  assert.equal(model.paginateTickets(rows.slice(0,1),4,10).currentPage,1);
});
test('Sort is numeric for ticket numbers and chronological for dates', () => {
  const numeric=select({sortBy:'id',direction:'asc'}).map(ticket=>ticket.id);assert.deepEqual(numeric,[...numeric].sort((a,b)=>a-b));
  const dates=select({sortBy:'createdAt',direction:'asc'}).map(ticket=>model.parseTicketDate(ticket.createdAt));assert.deepEqual(dates,[...dates].sort((a,b)=>a-b));
});
test('Workload matches legacy counting: recent active specialists, all in-progress tickets', () => {
  const now=Date.UTC(2026,9,6);const workload=model.specialistWorkload(tickets,specialists,now);
  assert.ok(workload.some(s=>s.count===0));assert.ok(workload[0].count>=workload[1].count);
  const oldClosed={...tickets[0],status:'Закрыта',createdAt:'01.01.2020'};
  assert.deepEqual(model.specialistWorkload([...tickets,oldClosed],specialists,now),workload);
  const oldActive={...tickets[0],createdAt:'01.01.2020'};
  const withOld=model.specialistWorkload([...tickets,oldActive],specialists,now);
  assert.equal(withOld.find(s=>s.id==='smirnova').count,workload.find(s=>s.id==='smirnova').count+1);
});
test('Only open tickets become overdue after the end of their deadline day', () => {
  const ticket={...tickets[0],deadline:'06.10.2026'};
  assert.equal(model.isOverdue(ticket,Date.UTC(2026,9,6,23,59)),false);
  assert.equal(model.isOverdue(ticket,Date.UTC(2026,9,7)),true);
  assert.equal(model.isOverdue({...ticket,status:'Закрыта'},Date.UTC(2026,9,7)),false);
});
test('Excel CSV includes all filtered rows, selected columns, BOM and safe escaped strings', () => {
  const csv=ticketsCsv([{...tickets[0],customer:'=HYPERLINK("bad")'}],['id','customer']);
  assert.ok(csv.startsWith('\uFEFF'));assert.ok(csv.includes('"\'=HYPERLINK(""bad"")"'));
  assert.equal(ticketsCsv(tickets,['id']).split('\r\n').length,tickets.length+1);
  assert.equal(csv.split('\r\n')[0],'\uFEFF"Номер заявки";"Название клиента"');
});
