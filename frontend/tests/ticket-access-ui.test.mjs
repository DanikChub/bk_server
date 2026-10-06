import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
import { loadUI } from './helpers/load-ui.mjs';
const file = name => fileURLToPath(new URL('../src/'+name,import.meta.url));
const {employeeTicketSession:employee,managerTicketSession:manager}=loadUI(file('shared/config/demo-ticket-sessions.ts'));
const {TicketActions}=loadUI(file('widgets/ticket-details/ui/ticket-actions.tsx'));
const {TicketConversation}=loadUI(file('widgets/ticket-details/ui/ticket-conversation.tsx'));
const {createTicketDetail}=loadUI(file('entities/ticket/model/detail.ts'));
const {tickets}=loadUI(file('entities/ticket/model/mock.ts'));
const {TicketDetailsPage}=loadUI(file('views/tickets/ui/ticket-details-page.tsx'));
const {TicketsPage}=loadUI(file('views/tickets/ui/tickets-page.tsx'));
const {DemoTicketSessionContext}=loadUI(file('shared/lib/access/demo-ticket-session.tsx'));
const detail=createTicketDetail(tickets[0]);
const noop=()=>{};
function actions(session){return renderToStaticMarkup(React.createElement(TicketActions,{session,detail,favorite:false,onFavorite:noop,onDelete:noop,onField:noop,onEvent:noop}));}
function conversation(session){return renderToStaticMarkup(React.createElement(TicketConversation,{session,messages:detail.messages,onDownload:noop,onDelete:noop,onEdit:noop}));}

test('The employee card renders usable fields and favorites with no deletion controls',()=>{
 const html=actions(employee);
 assert.ok(html.includes('В избранное'));assert.ok(html.includes('Сохранить: Специалист:'));
 assert.ok(!html.includes('Удалить'));assert.ok(!conversation(employee).includes('Удалить'));
 assert.ok(conversation(employee).includes('Редактировать сообщение'));
 assert.ok(actions(manager).includes('Удалить'));assert.ok(conversation(manager).includes('Удалить сообщение'));
});
test('Read-only policies remove save, composer and message editing controls from the rendered card',()=>{
 const session={...employee,grantedPolicies:['Ticket','Ticket.Get','TicketHistory.Get']};
 const html=actions(session);
 assert.ok(html.includes('ticket-action-readonly'));assert.ok(!html.includes('Сохранить:'));
 assert.ok(!conversation(session).includes('Редактировать сообщение'));
 const page=renderToStaticMarkup(React.createElement(DemoTicketSessionContext.Provider,{value:{session,employee:true,ticketsHref:'/employee/tickets'}},React.createElement(TicketDetailsPage,{ticket:tickets[0]})));
 assert.ok(!page.includes('aria-label="Ответ клиенту"'));assert.ok(!page.includes('aria-label="Сообщение коллеге"'));
});
test('The shared employee list scopes Mine and every card link to the current employee',()=>{
 const html=renderToStaticMarkup(React.createElement(DemoTicketSessionContext.Provider,{value:{session:employee,employee:true,ticketsHref:'/employee/tickets'}},React.createElement(TicketsPage)));
 const mine=tickets.filter(t=>t.responsibleId===employee.specialistId);
 assert.ok(mine.length>0);
 for(const ticket of mine)assert.ok(html.includes(`href="/employee/tickets/${ticket.id}"`));
 assert.ok(!html.includes('href="/tickets/'));assert.ok(!html.includes('ticket-specialists'));
 assert.ok(html.includes('Мои заявки'));assert.ok(html.includes('Новые'));assert.ok(html.includes('Избранное'));
});
