import { Mail, MailOpen, Inbox } from "lucide-react";
import type { NotificationCategory, NotificationQuery, NotificationStatus } from "@/entities/notification/model/notifications";
export const notificationStatusTabs = [
  { id: "unread", label: "Непрочитанные", icon: Mail },
  { id: "read", label: "Прочитанные", icon: MailOpen },
  { id: "all", label: "Все", icon: Inbox },
] as const;
export function NotificationFilters({ query, counts, categories, onStatus, onCategory }: {
  query: NotificationQuery; counts: Record<NotificationStatus, number>; categories: NotificationCategory[];
  onStatus: (status: NotificationStatus) => void; onCategory: (id: string) => void;
}) {
  return <aside className="panel notification-filters" aria-label="Фильтры уведомлений">
    <h2>По статусам</h2><nav aria-label="Статус уведомлений">{notificationStatusTabs.map(({ id, label, icon: Icon }) =>
      <button key={id} aria-pressed={query.status === id} onClick={() => onStatus(id)}><Icon size={15} /><span>{label}</span><strong>{counts[id]}</strong></button>
    )}</nav>
    <h2>По категориям</h2><nav aria-label="Категории уведомлений"><button aria-pressed={!query.categoryId} onClick={() => onCategory("")}>Все категории</button>{categories.map(category =>
      <button key={category.id} aria-pressed={query.categoryId === category.id} onClick={() => onCategory(category.id)}><span className="notification-category-dot" style={{ background: category.color }} />{category.name}</button>
    )}</nav>
  </aside>;
}
