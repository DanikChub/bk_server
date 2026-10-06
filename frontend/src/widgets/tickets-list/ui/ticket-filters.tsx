import type { Ticket } from "@/entities/ticket/model/types";
import { emptyFilters, type TicketFilters } from "../model/query";

type Props = { tickets: Ticket[]; filters: TicketFilters; onChange: (filters: TicketFilters) => void };
export function TicketFiltersPanel({ tickets, filters, onChange }: Props) {
  const update = (field: keyof TicketFilters, value: string) => onChange({ ...filters, [field]: value });
  const options = (field: "tag" | "type" | "section") => [...new Set(tickets.map(ticket => ticket[field]).filter(Boolean))].sort((a,b) => a.localeCompare(b, "ru"));
  return <fieldset className="ticket-filters" id="ticket-filters"><legend>Фильтры заявок</legend>
    <label>Номер заявки<input inputMode="numeric" value={filters.number} onChange={e => update("number", e.target.value)} /></label>
    <label>Название клиента<input value={filters.customer} onChange={e => update("customer", e.target.value)} /></label>
    {([['tag','Метка'],['type','Тип'],['section','Раздел']] as const).map(([field,label]) => <label key={field}>{label}<select value={filters[field]} onChange={e => update(field, e.target.value)}><option value="">Все</option>{options(field).map(value => <option key={value}>{value}</option>)}</select></label>)}
    <label>Ответственный<input value={filters.responsible} onChange={e => update("responsible", e.target.value)} /></label>
    <label>Дата создания<input type="date" value={filters.createdAt} onChange={e => update("createdAt", e.target.value)} /></label>
    <button className="secondary-button" onClick={() => onChange(emptyFilters)} type="button">Сбросить фильтры</button>
  </fieldset>;
}
