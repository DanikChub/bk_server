export type NotificationStatus = "unread" | "read" | "all";
export type NotificationCategory = { id: string; name: string; color: string };
export type UserNotification = {
  id: string;
  identityUserId: string;
  title: string;
  url: string | null;
  categoryId: string | null;
  createdAt: string;
  seen: string | null;
};
export type NotificationQuery = { status: NotificationStatus; categoryId: string; title: string };
export type NotificationReadState = Record<string, string>;
export const initialNotificationQuery: NotificationQuery = { status: "unread", categoryId: "", title: "" };

export function isNotificationReadState(value: unknown): value is NotificationReadState {
  return typeof value === "object" && value !== null && !Array.isArray(value)
    && Object.entries(value).every(([id, date]) => id.length > 0 && typeof date === "string" && Number.isFinite(Date.parse(date)));
}

export function notificationsForUser(rows: UserNotification[], identityUserId: string, seen: NotificationReadState = {}): UserNotification[] {
  if (!identityUserId) return [];
  return rows.filter(row => row.identityUserId === identityUserId).map(row => ({ ...row, seen: seen[row.id] ?? row.seen }));
}

export function selectNotifications(rows: UserNotification[], identityUserId: string, query: NotificationQuery): UserNotification[] {
  const title = query.title.trim().toLocaleLowerCase("ru-RU");
  return notificationsForUser(rows, identityUserId).filter(row =>
    (query.status === "all" || (query.status === "read" ? row.seen !== null : row.seen === null))
    && (!query.categoryId || row.categoryId === query.categoryId)
    && (!title || row.title.toLocaleLowerCase("ru-RU").includes(title))
  ).sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt) || a.id.localeCompare(b.id));
}

export function notificationCounts(rows: UserNotification[], identityUserId: string) {
  const own = notificationsForUser(rows, identityUserId);
  const unread = own.filter(row => row.seen === null).length;
  return { all: own.length, unread, read: own.length - unread };
}

export function markNotificationsRead(rows: UserNotification[], identityUserId: string, state: NotificationReadState, ids: readonly string[], now: string): NotificationReadState {
  if (!Number.isFinite(Date.parse(now))) throw new Error("Некорректная дата прочтения.");
  const selected = new Set(ids);
  const next = { ...state };
  for (const row of notificationsForUser(rows, identityUserId, state)) {
    if (selected.has(row.id) && row.seen === null) next[row.id] = now;
  }
  return next;
}

// Map legacy redirects to existing card routes, without accepting arbitrary URLs.
export function notificationTicketHref(url: string | null, ticketsHref: "/tickets" | "/employee/tickets", ticketIds: readonly number[]): string | null {
  if (!url || /[\\\u0000-\u001f]/.test(url)) return null;
  const match = /^\/(?:employee\/)?tickets\/(?:details\/|detailsMessage\/)?([1-9]\d*)(?:\?[^#]*)?$/i.exec(url);
  if (!match) return null;
  const id = Number(match[1]);
  return Number.isSafeInteger(id) && ticketIds.includes(id) ? `${ticketsHref}/${id}` : null;
}
