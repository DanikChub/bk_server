"use client";
import "@/widgets/notifications/ui/notifications.css";
import { useState } from "react";
import { CheckCheck, Search } from "lucide-react";
import { initialNotificationQuery, selectNotifications, type NotificationQuery } from "@/entities/notification/model/notifications";
import { notificationCategories } from "@/entities/notification/model/mock";
import { useDemoNotifications } from "@/entities/notification/model/use-demo-notifications";
import { tickets } from "@/entities/ticket/model/mock";
import { useDemoTicketSession } from "@/shared/lib/access/demo-ticket-session";
import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { Pagination } from "@/shared/ui/pagination/pagination";
import { paginateRows } from "@/shared/lib/pagination";
import { NotificationFilters, notificationStatusTabs } from "@/widgets/notifications/ui/notification-filters";
import { NotificationsTable } from "@/widgets/notifications/ui/notifications-table";
const ticketIds = tickets.map(ticket => ticket.id);
export function NotificationsPage() {
  const { session, employee } = useDemoTicketSession();
  const { rows, counts, markRead } = useDemoNotifications(session.userId);
  const [query, setQuery] = useState(initialNotificationQuery);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [selected, setSelected] = useState<string[]>([]);
  const [notice, setNotice] = useState("");
  const matching = selectNotifications(rows, session.userId, query);
  const pagination = paginateRows(matching, page, pageSize);
  const selectable = pagination.rows.filter(row => row.seen === null).map(row => row.id);
  const currentSelected = selected.filter(id => selectable.includes(id));
  const title = notificationStatusTabs.find(tab => tab.id === query.status)!.label;
  function update(patch: Partial<NotificationQuery>) { setQuery(current => ({ ...current, ...patch }));setPage(1);setSelected([]); }
  function read(ids: string[]) {
    const stored = markRead(ids);
    setSelected(current => current.filter(id => !ids.includes(id)));
    setNotice(stored ? "Уведомления отмечены прочитанными." : "Прочтение отмечено до обновления страницы: браузер не разрешает сохранять данные.");
  }
  return <div><PageHeading title="Мои уведомления" breadcrumbs={[{ label: "Уведомления" }]} />
    <div className="notifications-layout"><NotificationFilters query={query} counts={counts} categories={notificationCategories} onStatus={status => update({ status })} onCategory={categoryId => update({ categoryId })} />
      <section className="panel notifications-panel" aria-label="Список уведомлений"><div className="notifications-toolbar"><h2>{title} ({matching.length})</h2>
        <form className="notification-search" onSubmit={event => { event.preventDefault();update({ title: search }); }}><Search size={15} aria-hidden="true" /><input aria-label="Поиск по заголовку уведомления" placeholder="Поиск по уведомлениям" value={search} onChange={event => setSearch(event.target.value)} /><button className="primary-button">Найти</button></form>
      </div><div className="notification-bulk-actions"><label><input type="checkbox" aria-label="Выбрать непрочитанные уведомления на этой странице" disabled={selectable.length === 0} checked={selectable.length > 0 && currentSelected.length === selectable.length} onChange={() => setSelected(currentSelected.length === selectable.length ? [] : selectable)} />Выбрать на странице</label><button className="secondary-button" disabled={currentSelected.length === 0} onClick={() => read(currentSelected)}><CheckCheck size={14} />Отметить выбранные прочитанными</button>{(query.title || query.categoryId) && <button className="notification-reset" onClick={() => { setSearch("");update({ title: "", categoryId: "" }); }}>Сбросить поиск и категорию</button>}</div>
        <NotificationsTable rows={pagination.rows} categories={notificationCategories} selected={currentSelected} ticketsHref={employee ? "/employee/tickets" : "/tickets"} ticketIds={ticketIds} onSelect={id => setSelected(current => current.includes(id) ? current.filter(value => value !== id) : [...current, id])} onRead={read} />
        <Pagination page={pagination.currentPage} pageCount={pagination.pageCount} pageSize={pageSize} total={matching.length} navigationLabel="Страницы уведомлений" onPageChange={value => { setPage(value);setSelected([]); }} onPageSizeChange={value => { setPageSize(value);setPage(1);setSelected([]); }} />
      </section></div><p className="notifications-notice" role="status">{notice || "Демонстрационный режим · прочтение сохраняется отдельно для текущего пользователя в этом браузере."}</p>
    </div>;
}
