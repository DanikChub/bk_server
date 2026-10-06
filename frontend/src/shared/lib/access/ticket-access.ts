export type TicketSession = {
  userId: string;
  specialistId: string;
  displayName: string;
  roles: readonly string[];
  grantedPolicies: readonly string[];
};

export function hasPolicy(session: TicketSession, policy: string): boolean {
  return session.grantedPolicies.includes(policy);
}

// Mirror service authorization, including the legacy Razor role restriction.
// These checks govern the UI; the API must independently authorize every request.
export function ticketAccess(session: TicketSession) {
  const read = hasPolicy(session, "Ticket") && hasPolicy(session, "Ticket.Get");
  return {
    read,
    edit: read && hasPolicy(session, "Ticket.Edit"),
    reply: read && hasPolicy(session, "TicketHistory.Create"),
    readHistory: read && hasPolicy(session, "TicketHistory.Get"),
    editHistory: read && hasPolicy(session, "TicketHistory.Edit"),
    // Razor checks TicketHistory.Delete; the service checks Ticket.Delete.
    deleteHistory: read && hasPolicy(session, "TicketHistory.Delete") && hasPolicy(session, "Ticket.Delete"),
    deleteTicket: read && hasPolicy(session, "Ticket.Delete") && session.roles.some(role => role === "admin" || role === "Руководитель отдела продаж"),
    // The Razor ticket page is authenticated; CreateEventAsync has no named grant.
    readEvents: read && hasPolicy(session, "Event.Get"),
    createEvent: read && Boolean(session.userId),
  };
}

export function canEditTicketMessage(session: TicketSession, message: { id: string; kind: string }): boolean {
  const access = ticketAccess(session);
  if (message.id === "initial") return access.editHistory && access.edit;
  return access.editHistory && (message.kind === "reply" || message.kind === "internal");
}

export function canDeleteTicketMessage(session: TicketSession, message: { id: string; kind: string }): boolean {
  return message.id !== "initial" && ticketAccess(session).deleteHistory;
}

export function personalTicketKey(session: Pick<TicketSession, "specialistId" | "userId">, kind: "favorites" | "columns"): string {
  return `tickets:${kind}:demo:${session.specialistId || session.userId}:v1`;
}
