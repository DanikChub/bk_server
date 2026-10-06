"use client";

import "@/widgets/tickets-list/ui/tickets-list.css";
import { useState } from "react";
import { Download, Filter, Search, SlidersHorizontal, SortAsc, SortDesc } from "lucide-react";
import { demoSpecialistId, initialFavoriteTicketIds, specialists, tickets } from "@/entities/ticket/model/mock";
import { useStoredValue } from "@/shared/lib/use-stored-value";
import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { Pagination } from "@/shared/ui/pagination/pagination";
import { emptyFilters, initialQuery, paginateTickets, selectTickets, specialistWorkload, ticketColumns, ticketTabs, type TicketColumn, type TicketQuery } from "@/widgets/tickets-list/model/query";
import { downloadTickets } from "@/widgets/tickets-list/model/export";
import { ColumnSettings } from "@/widgets/tickets-list/ui/column-settings";
import { SpecialistsList } from "@/widgets/tickets-list/ui/specialists-list";
import { TicketFiltersPanel } from "@/widgets/tickets-list/ui/ticket-filters";
import { TicketsTable } from "@/widgets/tickets-list/ui/tickets-table";

const allColumns = ticketColumns.map(column => column.id);
function validColumns(value: unknown): value is TicketColumn[] {
  return Array.isArray(value) && value.includes("id") && new Set(value).size === value.length && value.every(id => allColumns.includes(id));
}
function validFavorites(value: unknown): value is number[] {
  return Array.isArray(value) && value.every(id => Number.isSafeInteger(id) && id > 0);
}

export function TicketsPage() {
  const [query, setQuery] = useState(initialQuery);
  const [searchInput, setSearchInput] = useState("");
  const [panel, setPanel] = useState<"filters" | "columns" | null>(null);
  const [favoriteFallback, setFavoriteFallback] = useState(initialFavoriteTicketIds);
  const [columnFallback, setColumnFallback] = useState(allColumns);
  const [storageWarning, setStorageWarning] = useState(false);
  const [referenceTime] = useState(() => Date.now());
  const [favoriteIds, saveFavorites] = useStoredValue(`tickets:favorites:demo:${demoSpecialistId}:v1`, favoriteFallback, validFavorites);
  const [visibleColumns, saveColumns] = useStoredValue(`tickets:columns:demo:${demoSpecialistId}:v1`, columnFallback, validColumns);
  const rows = selectTickets(tickets, query, demoSpecialistId, favoriteIds);
  const pagination = paginateTickets(rows, query.page, query.pageSize);
  const workload = specialistWorkload(tickets, specialists, referenceTime);
  const activeTab = ticketTabs.find(tab => tab.id === query.tab)!;
  const activeFilters = Object.values(query.filters).filter(value => value.trim()).length;
  const hasSearch = Boolean(query.search || activeFilters || query.specialistId);

  function updateQuery(patch: Partial<TicketQuery>) {
    setQuery(current => ({ ...current, ...patch, page: 1 }));
  }
  function changeColumns(columns: TicketColumn[]) {
    setColumnFallback(columns);
    if (!saveColumns(columns)) setStorageWarning(true);
  }
  function toggleFavorite(id: number) {
    const next = favoriteIds.includes(id) ? favoriteIds.filter(value => value !== id) : [...favoriteIds, id];
    setFavoriteFallback(next);
    if (!saveFavorites(next)) setStorageWarning(true);
  }
  function resetSearch() {
    setSearchInput("");updateQuery({ search: "", filters: emptyFilters, specialistId: "" });
  }
  function sort(column: TicketColumn) {
    updateQuery({ sortBy: column, direction: query.sortBy === column && query.direction === "asc" ? "desc" : "asc" });
  }

  return <div>
    <PageHeading title={`${activeTab.label} (${rows.length})`} breadcrumbs={[{ label: "Заявки", href: "/tickets" }, { label: activeTab.label }]} />
    <div className="manager-tickets-layout">
      <section className="panel manager-tickets-panel" aria-label="Список заявок">
        <div className="manager-ticket-toolbar">
          <div className="toolbar-group">
            <button className="secondary-button" onClick={() => downloadTickets(rows, visibleColumns)} title="Скачать все найденные заявки в CSV для Excel" disabled={rows.length === 0}><Download size={14} />Открыть в Excel</button>
            <button className="secondary-button" onClick={() => setPanel(panel === "columns" ? null : "columns")} aria-expanded={panel === "columns"} aria-controls="ticket-column-settings"><SlidersHorizontal size={14} />Настройка колонок</button>
            <button className="icon-button" aria-label="Сортировать по возрастанию" aria-pressed={query.direction === "asc"} onClick={() => updateQuery({ direction: "asc" })}><SortAsc size={14} /></button>
            <button className="icon-button" aria-label="Сортировать по убыванию" aria-pressed={query.direction === "desc"} onClick={() => updateQuery({ direction: "desc" })}><SortDesc size={14} /></button>
          </div>
          <form className="manager-ticket-search" onSubmit={event => { event.preventDefault();updateQuery({ search: searchInput.trim() }); }}>
            <button className={`icon-button ${activeFilters ? "has-active-filters" : ""}`} type="button" aria-label={`Фильтры${activeFilters ? `: активно ${activeFilters}` : ""}`} aria-expanded={panel === "filters"} aria-controls="ticket-filters" onClick={() => setPanel(panel === "filters" ? null : "filters")}><Filter size={14} /></button>
            <div className="ticket-search-input"><Search size={14} aria-hidden="true" /><input aria-label="Поиск по номеру заявки или названию клиента" placeholder="Поиск" value={searchInput} onChange={event => setSearchInput(event.target.value)} /><button type="submit">Найти</button></div>
          </form>
        </div>
        {panel === "columns" && <ColumnSettings visible={visibleColumns} onChange={changeColumns} />}
        {panel === "filters" && <TicketFiltersPanel tickets={tickets} filters={query.filters} onChange={filters => updateQuery({ filters })} />}
        <nav className="manager-ticket-tabs" aria-label="Категории заявок">{ticketTabs.map(tab => <button key={tab.id} aria-pressed={query.tab === tab.id} onClick={() => updateQuery({ tab: tab.id })}>{tab.label}</button>)}</nav>
        {hasSearch && <div className="ticket-active-search"><span>Найдено: {rows.length}{query.search && ` · Поиск: «${query.search}»`}{query.specialistId && ` · ${specialists.find(item => item.id === query.specialistId)?.name}`}{activeFilters > 0 && ` · Фильтров: ${activeFilters}`}</span><button onClick={resetSearch}>Сбросить поиск и фильтры</button></div>}
        {storageWarning && <p className="ticket-storage-warning" role="status">Браузер не разрешает сохранять настройки. Изменения действуют до обновления страницы.</p>}
        <TicketsTable rows={pagination.rows} query={query} visibleColumns={visibleColumns} favoriteIds={favoriteIds} onFavorite={toggleFavorite} onSort={sort} referenceTime={referenceTime} />
        <Pagination page={pagination.currentPage} pageCount={pagination.pageCount} pageSize={query.pageSize} total={rows.length} onPageChange={page => setQuery(current => ({ ...current, page }))} onPageSizeChange={pageSize => updateQuery({ pageSize })} />
      </section>
      <SpecialistsList specialists={workload} selectedId={query.specialistId} onSelect={specialistId => updateQuery({ specialistId, tab: specialistId ? "all" : query.tab })} />
    </div>
    <p className="tickets-demo-note">Демонстрационный режим · избранное и настройки колонок сохраняются в этом браузере.</p>
  </div>;
}
