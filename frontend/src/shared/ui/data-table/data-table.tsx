import Link from "next/link";
import type { ReactNode } from "react";

export type Column<T> = { title: string; render: (row: T) => ReactNode; className?: string; header?: ReactNode; sortDirection?: "asc" | "desc" };

export function DataTable<T extends { id: string | number }>({ rows, columns, href, rowClassName, emptyMessage = "Нет записей" }: { rows: T[]; columns: Column<T>[]; href?: (row: T) => string; rowClassName?: (row: T) => string; emptyMessage?: string }) {
  return (
    <div className="table-scroll">
      <table className="data-table">
        <thead><tr>{columns.map((column) => <th key={column.title} className={column.className} scope="col" aria-sort={column.sortDirection === "asc" ? "ascending" : column.sortDirection === "desc" ? "descending" : undefined}>{column.header ?? column.title}</th>)}</tr></thead>
        <tbody>
          {rows.length === 0 && <tr><td colSpan={columns.length} className="table-empty">{emptyMessage}</td></tr>}
          {rows.map((row) => (
            <tr key={row.id} className={rowClassName?.(row)}>
              {columns.map((column, index) => (
                <td key={column.title} className={column.className}>
                  {index === 0 && href ? <Link className="table-link" href={href(row)}>{column.render(row)}</Link> : column.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
