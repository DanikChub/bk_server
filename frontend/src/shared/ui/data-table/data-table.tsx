import Link from "next/link";
import type { ReactNode } from "react";

export type Column<T> = { title: string; render: (row: T) => ReactNode; className?: string };

export function DataTable<T extends { id: string | number }>({ rows, columns, href }: { rows: T[]; columns: Column<T>[]; href?: (row: T) => string }) {
  return (
    <div className="table-scroll">
      <table className="data-table">
        <thead><tr>{columns.map((column) => <th key={column.title} className={column.className}>{column.title}</th>)}</tr></thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id}>
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
