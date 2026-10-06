import type { TicketSession } from "@/shared/lib/access/ticket-access";

// Review fixtures, NOT authenticated roles or a claim about deployed role grants.
// The legacy repository defines policies but does not contain role grant data.
export const managerTicketSession: TicketSession = {
  userId: "demo-morozov", specialistId: "morozov", displayName: "Морозов Иван",
  roles: ["Руководитель отдела продаж"],
  grantedPolicies: ["Ticket", "Ticket.Get", "Ticket.Edit", "Ticket.Delete", "TicketHistory.Get", "TicketHistory.Create", "TicketHistory.Edit", "TicketHistory.Delete", "Event.Get", "TicketSection", "ContractStatuses"],
};
export const employeeTicketSession: TicketSession = {
  userId: "demo-makarovsky", specialistId: "makarovsky", displayName: "Макаровский Вадим",
  roles: ["Сотрудник"],
  grantedPolicies: ["Ticket", "Ticket.Get", "Ticket.Edit", "TicketHistory.Get", "TicketHistory.Create", "TicketHistory.Edit", "Event.Get"],
};
