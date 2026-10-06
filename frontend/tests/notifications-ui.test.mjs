import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
import { loadUI } from './helpers/load-ui.mjs';
const file=name=>fileURLToPath(new URL('../src/'+name,import.meta.url));
const {NotificationsPage}=loadUI(file('views/notifications/ui/notifications-page.tsx'));
const {NotificationsTable}=loadUI(file('widgets/notifications/ui/notifications-table.tsx'));
const {DemoTicketSessionContext}=loadUI(file('shared/lib/access/demo-ticket-session.tsx'));
const {employeeTicketSession:session}=loadUI(file('shared/config/demo-ticket-sessions.ts'));
const {demoNotifications,notificationCategories}=loadUI(file('entities/notification/model/mock.ts'));

test('The employee notification page has personal unread links, categories, search and notification pagination labels',()=>{
 const html=renderToStaticMarkup(React.createElement(DemoTicketSessionContext.Provider,{value:{session,employee:true,ticketsHref:'/employee/tickets'}},React.createElement(NotificationsPage)));
 assert.ok(html.includes('href="/employee/tickets/128569"'));assert.ok(html.includes('href="/employee/tickets/128548"'));
 assert.ok(!html.includes('href="/tickets/'));assert.ok(html.includes('Поиск по заголовку уведомления'));
 assert.ok(html.includes('Категории уведомлений'));assert.ok(html.includes('Страницы уведомлений'));
});
test('Unavailable notification destinations render no unsafe links and still offer marking read',()=>{
 const row={...demoNotifications[0],url:'javascript:alert(1)'};
 const html=renderToStaticMarkup(React.createElement(NotificationsTable,{rows:[row],categories:notificationCategories,selected:[],ticketsHref:'/tickets',ticketIds:[128569],onRead:()=>{},onSelect:()=>{}}));
 assert.ok(!html.includes('href='));assert.ok(!html.includes('javascript:'));assert.ok(html.includes('Прочитать'));assert.ok(html.includes('Связанная заявка недоступна'));
});
