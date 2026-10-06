import type { Ticket } from "@/entities/ticket/model/types";
import { ticketColumns, type TicketColumn } from "./query";
export function ticketsCsv(rows: Ticket[], visibleColumns: TicketColumn[]): string {
  const columns = ticketColumns.filter(column => visibleColumns.includes(column.id));
  const escape = (value: string) => `"${(/^[=+@\-\t\r]/.test(value) ? "'" : "") + value.replaceAll('"', '""')}"`;
  return "\uFEFF" + [columns.map(column => escape(column.label)).join(";"), ...rows.map(ticket => columns.map(column => escape(String(ticket[column.id]))).join(";"))].join("\r\n");
}
export function downloadTickets(rows: Ticket[], visibleColumns: TicketColumn[]) {
  const url = URL.createObjectURL(new Blob([ticketsCsv(rows, visibleColumns)], { type: "text/csv;charset=utf-8" }));
  const anchor = document.createElement("a");anchor.href = url;anchor.download = "заявки.csv";anchor.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
