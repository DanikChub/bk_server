import type { Specialist, Ticket } from "@/entities/ticket/model/types";

export const ticketTabs = [
  { id: "mine", label: "Мои заявки" },
  { id: "new", label: "Новые" },
  { id: "active", label: "В работе" },
  { id: "reopened", label: "Возобновлённые" },
  { id: "closed", label: "Закрытые" },
  { id: "all", label: "Все" },
  { id: "favorites", label: "Избранное" },
] as const;
export type TicketTab = typeof ticketTabs[number]["id"];
export type TicketColumn = "status" | "id" | "customer" | "tag" | "type" | "section" | "createdAt" | "responsible";
export const ticketColumns: { id: TicketColumn; label: string }[] = [
  { id: "status", label: "Статус" }, { id: "id", label: "Номер заявки" },
  { id: "customer", label: "Название клиента" }, { id: "tag", label: "Метка" },
  { id: "type", label: "Тип" }, { id: "section", label: "Раздел" },
  { id: "createdAt", label: "Дата создания" }, { id: "responsible", label: "Ответственный" },
];
export type TicketFilters = { number: string; customer: string; tag: string; type: string; section: string; responsible: string; createdAt: string };
export const emptyFilters: TicketFilters = { number: "", customer: "", tag: "", type: "", section: "", responsible: "", createdAt: "" };
export type TicketQuery = { tab: TicketTab; search: string; filters: TicketFilters; specialistId: string; sortBy: TicketColumn; direction: "asc" | "desc"; page: number; pageSize: number };
export const initialQuery: TicketQuery = { tab: "mine", search: "", filters: emptyFilters, specialistId: "", sortBy: "id", direction: "desc", page: 1, pageSize: 10 };

const normalize = (value: string) => value.trim().toLocaleLowerCase("ru").replaceAll("ё", "е");
const contains = (value: string, query: string) => normalize(value).includes(normalize(query));
export function parseTicketDate(value: string): number {
  const [day, month, year] = value.split(" ")[0].split(".").map(Number);
  return Date.UTC(year, month - 1, day);
}
export function isOverdue(ticket: Ticket, now = Date.now()): boolean {
  return ticket.status !== "Закрыта" && parseTicketDate(ticket.deadline) + 86400000 <= now;
}
export function selectTickets(tickets: Ticket[], query: TicketQuery, currentSpecialistId: string, favoriteIds: number[]): Ticket[] {
  const f = query.filters;
  return tickets.filter(ticket => {
    const tabMatch = query.tab === "mine" ? ticket.responsibleId === currentSpecialistId
      : query.tab === "favorites" ? favoriteIds.includes(ticket.id)
      : query.tab === "new" ? ticket.status === "Новая"
      : query.tab === "active" ? ticket.status === "В работе"
      : query.tab === "reopened" ? ticket.status === "Возобновлённая"
      : query.tab === "closed" ? ticket.status === "Закрыта" : true;
    // The old backend's general search matches client name or exact ticket ID.
    return tabMatch && (!query.specialistId || ticket.responsibleId === query.specialistId)
      && (!query.search.trim() || contains(ticket.customer, query.search) || ticket.id === Number(query.search.replaceAll(" ", "")))
      && (!f.number.trim() || ticket.id === Number(f.number.replaceAll(" ", "")))
      && contains(ticket.customer, f.customer) && contains(ticket.responsible, f.responsible)
      && (!f.tag || ticket.tag === f.tag) && (!f.type || ticket.type === f.type) && (!f.section || ticket.section === f.section)
      && (!f.createdAt || parseTicketDate(ticket.createdAt) === Date.parse(f.createdAt + "T00:00:00Z"));
  }).sort((a, b) => {
    const field = query.sortBy;
    const compared = field === "id" ? a.id - b.id : field === "createdAt" ? parseTicketDate(a.createdAt) - parseTicketDate(b.createdAt)
      : a[field].localeCompare(b[field], "ru", { numeric: true });
    return (compared || a.id - b.id) * (query.direction === "asc" ? 1 : -1);
  });
}
export function paginateTickets(rows: Ticket[], page: number, pageSize: number) {
  const pageCount = Math.max(1, Math.ceil(rows.length / pageSize));
  const currentPage = Math.max(1, Math.min(page, pageCount));
  return { rows: rows.slice((currentPage - 1) * pageSize, currentPage * pageSize), currentPage, pageCount };
}
export function specialistWorkload(tickets: Ticket[], specialists: Specialist[], now = Date.now()) {
  const start = now - 30 * 86400000;
  // Legacy API: identify specialists active in the period, then count ALL
  // their current in-progress tickets (not just tickets created in the period).
  const active = tickets.filter(ticket => ticket.status === "В работе");
  return specialists.map(specialist => {
    const hasRecentActivity = active.some(ticket => ticket.responsibleId === specialist.id && parseTicketDate(ticket.createdAt) > start && parseTicketDate(ticket.createdAt) < now);
    return { ...specialist, count: hasRecentActivity ? active.filter(ticket => ticket.responsibleId === specialist.id).length : 0 };
  }).sort((a, b) => b.count - a.count || a.name.localeCompare(b.name, "ru")).slice(0, 27);
}
