import type { NotificationCategory, UserNotification } from "./notifications";

// The deployed categories are database records; these are review fixtures only.
export const notificationCategories: NotificationCategory[] = [
  { id: "tickets", name: "Заявки", color: "#1ab394" },
  { id: "events", name: "События", color: "#f8ac59" },
  { id: "system", name: "Системные", color: "#23c6c8" },
];
const ids = [128569, 128548, 128527, 1842, 1841, 1838, 128541, 128520, 128513, 128506, 128499, 1829];
export const demoNotifications: UserNotification[] = ["demo-morozov", "demo-makarovsky"].flatMap(identityUserId =>
  ids.map((ticketId, index) => ({
    id: `${identityUserId}-${index + 1}`, identityUserId,
    title: index === 0 ? `Ответ по заявке ${ticketId}` : index === 1 ? `Сообщение по заявке ${ticketId}` : index % 3 === 0 ? `Напоминание по заявке ${ticketId}` : index % 3 === 1 ? `Заявка ${ticketId} назначена специалисту` : "Обновлена библиотека консультаций",
    url: index % 3 === 2 ? null : `/tickets/${index === 1 ? "detailsMessage/" : "details/"}${ticketId}`,
    categoryId: index < 2 ? "tickets" : notificationCategories[index % 3].id,
    createdAt: `2026-10-${String(6 - Math.floor(index / 3)).padStart(2, "0")}T${String(10 - index % 3).padStart(2, "0")}:30:00+03:00`,
    seen: index < 2 ? null : "2026-10-06T00:00:00Z",
  }))
);
