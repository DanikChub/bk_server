import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { loadModel } from './helpers/load-model.mjs';
const model = loadModel(fileURLToPath(new URL('../src/shared/lib/access/ticket-access.ts', import.meta.url)));
const session = { userId:'employee-user', specialistId:'employee-profile', displayName:'Сотрудник', roles:['Сотрудник'], grantedPolicies:['Ticket', 'Ticket.Get', 'Ticket.Edit', 'TicketHistory.Get', 'TicketHistory.Create', 'TicketHistory.Edit', 'Event.Get'] };
const withPolicies = (...policies) => ({...session, grantedPolicies:policies});

test('Staff can read, edit fields and reply with the corresponding service policies, but cannot delete', () => {
 const access=model.ticketAccess(session);
 assert.equal(access.read,true);assert.equal(access.edit,true);assert.equal(access.reply,true);assert.equal(access.readHistory,true);
 assert.equal(access.deleteTicket,false);assert.equal(access.deleteHistory,false);
});
test('Role names alone do not grant ticket access or mutation rights', () => {
 const access=model.ticketAccess({...session,roles:['admin'],grantedPolicies:[]});
 for(const value of Object.values(access))assert.equal(value,false);
});
test('Deleting a ticket requires both the legacy manager role and Ticket.Delete', () => {
 const policies=[...session.grantedPolicies,'Ticket.Delete'];
 assert.equal(model.ticketAccess({...session,grantedPolicies:policies}).deleteTicket,false);
 assert.equal(model.ticketAccess({...session,roles:['admin'],grantedPolicies:policies}).deleteTicket,true);
 assert.equal(model.ticketAccess({...session,roles:['Руководитель отдела продаж'],grantedPolicies:policies}).deleteTicket,true);
 assert.equal(model.ticketAccess({...session,roles:['Руководитель отдела продаж']}).deleteTicket,false);
});
test('History deletion respects both differing Razor and service permission names', () => {
 assert.equal(model.ticketAccess({...session,grantedPolicies:[...session.grantedPolicies,'TicketHistory.Delete']}).deleteHistory,false);
 assert.equal(model.ticketAccess({...session,grantedPolicies:[...session.grantedPolicies,'Ticket.Delete']}).deleteHistory,false);
 const permitted={...session,grantedPolicies:[...session.grantedPolicies,'Ticket.Delete','TicketHistory.Delete']};
 assert.equal(model.canDeleteTicketMessage(permitted,{id:'initial',kind:'client'}),false);
 assert.equal(model.canDeleteTicketMessage(permitted,{id:'reply',kind:'reply'}),true);
});
test('Original description needs the service Ticket.Edit grant as well as the Razor history grant', () => {
 assert.equal(model.canEditTicketMessage(session,{id:'initial',kind:'client'}),true);
 const historyOnly=withPolicies('Ticket','Ticket.Get','TicketHistory.Edit');
 assert.equal(model.canEditTicketMessage(historyOnly,{id:'initial',kind:'client'}),false);
 assert.equal(model.canEditTicketMessage(historyOnly,{id:'reply',kind:'reply'}),true);
 assert.equal(model.canEditTicketMessage(historyOnly,{id:'system',kind:'event'}),false);
});
test('Being a message creator cannot bypass the service authorization', () => {
 const readOnly=withPolicies('Ticket','Ticket.Get','TicketHistory.Get');
 assert.equal(model.canEditTicketMessage(readOnly,{id:'own',kind:'internal',creatorId:session.userId}),false);
 assert.equal(model.ticketAccess(readOnly).reply,false);
 assert.equal(model.ticketAccess(readOnly).edit,false);
});
test('Favorite and column keys follow the specialist identity, not the role or active page', () => {
 assert.equal(model.personalTicketKey(session,'favorites'),model.personalTicketKey({...session,roles:['admin']},'favorites'));
 assert.notEqual(model.personalTicketKey(session,'favorites'),model.personalTicketKey({...session,specialistId:'another-user'},'favorites'));
 assert.equal(model.personalTicketKey({...session,specialistId:''},'columns'),'tickets:columns:demo:employee-user:v1');
});
