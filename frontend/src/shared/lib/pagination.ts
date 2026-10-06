export function paginateRows<T>(rows: T[], page: number, pageSize: number) {
  const pageCount = Math.max(1, Math.ceil(rows.length / pageSize));
  const currentPage = Math.max(1, Math.min(page, pageCount));
  return { rows: rows.slice((currentPage - 1) * pageSize, currentPage * pageSize), currentPage, pageCount };
}
