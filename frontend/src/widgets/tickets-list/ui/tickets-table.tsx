import Link from "next/link";
import { Bookmark, CheckSquare, CircleAlert, CircleHelp, X, Zap } from "lucide-react";
import type { Ticket, TicketStatus } from "@/entities/ticket/model/types";
import { DataTable, type Column } from "@/shared/ui/data-table/data-table";
import { isOverdue, ticketColumns, type TicketColumn, type TicketQuery } from "../model/query";

const statusIcons: Record<TicketStatus, typeof Zap> = { "Новая": Zap, "В работе": CheckSquare, "Возобновлённая": CircleAlert, "Закрыта": X, "Требует уточнения": CircleHelp };
function TicketStatusIcon({ status }: { status: TicketStatus }) {
  const Icon = statusIcons[status];
  return <span className={`ticket-status ticket-status--${status === "Закрыта" ? "closed" : status === "Возобновлённая" ? "reopened" : "active"}`} title={status}><Icon size={17} aria-hidden="true" /><span className="sr-only">{status}</span></span>;
}
export function TicketsTable({ rows, visibleColumns, query, favoriteIds, onFavorite, onSort, referenceTime, ticketsHref = "/tickets" }: { rows: Ticket[]; visibleColumns: TicketColumn[]; query: TicketQuery; favoriteIds: number[]; onFavorite: (id: number) => void; onSort: (column: TicketColumn) => void; referenceTime: number; ticketsHref?: string }) {
  const columns: Column<Ticket>[] = ticketColumns.filter(column => visibleColumns.includes(column.id)).map(column => ({
    title: column.label,
    className: `ticket-column-${column.id}`,
    sortDirection: query.sortBy === column.id ? query.direction : undefined,
    header: <button className="ticket-sort" onClick={() => onSort(column.id)}>{column.label}<span aria-hidden="true" className="ticket-sort-indicator">{query.sortBy === column.id ? query.direction === "asc" ? "↑" : "↓" : ""}</span></button>,
    render: ticket => column.id === "status" ? <TicketStatusIcon status={ticket.status} />
      : column.id === "id" ? <div className="ticket-number"><Link href={`${ticketsHref}/${ticket.id}`} className="table-link">{ticket.id.toLocaleString("ru-RU")}</Link><button className={`ticket-favorite ${favoriteIds.includes(ticket.id) ? "is-favorite" : ""}`} onClick={() => onFavorite(ticket.id)} aria-pressed={favoriteIds.includes(ticket.id)} aria-label={`${favoriteIds.includes(ticket.id) ? "Убрать из избранного" : "В избранное"}: заявка ${ticket.id}`}><Bookmark size={13} /></button></div>
      : <span className="ticket-cell-text" title={column.id === "createdAt" ? ticket.createdAt : ticket[column.id]}>{column.id === "createdAt" ? ticket.createdAt.split(" ")[0] : ticket[column.id] || "—"}</span>,
  }));
  return <DataTable rows={rows} columns={columns} rowClassName={ticket => isOverdue(ticket, referenceTime) ? "ticket-overdue" : ""} emptyMessage="Заявки не найдены. Измените вкладку или условия поиска." />;
}
