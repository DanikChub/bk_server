import { ticketColumns, type TicketColumn } from "../model/query";
export function ColumnSettings({ visible, onChange }: { visible: TicketColumn[]; onChange: (columns: TicketColumn[]) => void }) {
  return <fieldset className="ticket-column-settings" id="ticket-column-settings"><legend>Настройка колонок</legend>{ticketColumns.map(column => <label key={column.id}><input type="checkbox" checked={visible.includes(column.id)} disabled={column.id === "id"} onChange={event => onChange(event.target.checked ? [...visible, column.id] : visible.filter(id => id !== column.id))} />{column.label}</label>)}<button type="button" className="secondary-button" onClick={() => onChange(ticketColumns.map(column => column.id))}>Показать все</button></fieldset>;
}
