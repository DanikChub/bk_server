import Link from "next/link";
import { Check, ExternalLink } from "lucide-react";
import { DataTable, type Column } from "@/shared/ui/data-table/data-table";
import { notificationTicketHref, type NotificationCategory, type UserNotification } from "@/entities/notification/model/notifications";
export function NotificationsTable({ rows, categories, selected, ticketsHref, ticketIds, onSelect, onRead }: {
  rows: UserNotification[]; categories: NotificationCategory[]; selected: string[];
  ticketsHref: "/tickets" | "/employee/tickets"; ticketIds: number[];
  onSelect: (id: string) => void; onRead: (ids: string[]) => void;
}) {
  const columns: Column<UserNotification>[] = [
    { title: "Выбрать", className: "notification-column-select", render: row => <input type="checkbox" aria-label={`Выбрать: ${row.title}`} disabled={row.seen !== null} checked={selected.includes(row.id)} onChange={() => onSelect(row.id)} /> },
    { title: "Уведомление", render: row => {
      const href = notificationTicketHref(row.url, ticketsHref, ticketIds);
      return <div className="notification-title">{row.seen === null && <span className="notification-unread-dot" title="Не прочитано" />}<span>{href ? <Link className="table-link" href={href} onClick={() => onRead([row.id])} onAuxClick={event => { if (event.button === 1) onRead([row.id]); }}>{row.title}<ExternalLink size={12} aria-hidden="true" /></Link> : row.title}{row.url && !href && <small>Связанная заявка недоступна</small>}</span></div>;
    } },
    { title: "Категория", className: "notification-column-category", render: row => { const category = categories.find(item => item.id === row.categoryId);return category ? <span className="notification-category" style={{ background: category.color }}>{category.name}</span> : "—"; } },
    { title: "Дата", className: "notification-column-date", render: row => <time dateTime={row.createdAt}>{new Date(row.createdAt).toLocaleString("ru-RU", { timeZone: "Europe/Moscow", dateStyle: "short", timeStyle: "short" })}</time> },
    { title: "Прочтение", className: "notification-column-read", render: row => row.seen ? <span className="notification-read" title={`Прочитано ${new Date(row.seen).toLocaleString("ru-RU", { timeZone: "Europe/Moscow" })}`}><Check size={14} />Прочитано</span> : <button className="secondary-button" onClick={() => onRead([row.id])}><Check size={13} />Прочитать</button> },
  ];
  return <DataTable rows={rows} columns={columns} rowClassName={row => row.seen === null ? "notification-row-unread" : ""} emptyMessage="Уведомлений нет. Измените статус, категорию или условия поиска." />;
}
