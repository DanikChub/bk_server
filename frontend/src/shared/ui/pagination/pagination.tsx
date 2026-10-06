import "./pagination.css";

type PaginationProps = { page: number; pageCount: number; pageSize: number; navigationLabel?: string; total: number; onPageChange: (page: number) => void; onPageSizeChange: (size: number) => void };
export function Pagination({ page, pageCount, pageSize, navigationLabel = "Страницы заявок", total, onPageChange, onPageSizeChange }: PaginationProps) {
  const start = Math.max(1, Math.min(page - 2, pageCount - 4));
  const pages = Array.from({ length: Math.min(5, pageCount) }, (_, index) => start + index);
  return <div className="ticket-pagination">
    <label>Показать <select aria-label="Количество записей на странице" value={pageSize} onChange={event => onPageSizeChange(Number(event.target.value))}>{[10,25,50,75,100].map(size => <option key={size}>{size}</option>)}</select> записей</label>
    <span className="ticket-results-count" role="status">{total ? `${(page - 1) * pageSize + 1}–${Math.min(page * pageSize, total)} из ${total}` : "Нет записей"}</span>
    <nav aria-label={navigationLabel}><button disabled={page === 1} onClick={() => onPageChange(1)} aria-label="Первая страница">«</button><button disabled={page === 1} onClick={() => onPageChange(page - 1)} aria-label="Предыдущая страница">‹</button>{pages.map(number => <button key={number} aria-current={number === page ? "page" : undefined} onClick={() => onPageChange(number)}>{number}</button>)}<button disabled={page === pageCount} onClick={() => onPageChange(page + 1)} aria-label="Следующая страница">›</button><button disabled={page === pageCount} onClick={() => onPageChange(pageCount)} aria-label="Последняя страница">»</button></nav>
  </div>;
}
