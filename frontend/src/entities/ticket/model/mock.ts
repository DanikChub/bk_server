export type Ticket = {
  id: number;
  subject: string;
  customer: string;
  author: string;
  responsible: string;
  status: "Новая" | "В работе" | "Ожидает" | "Закрыта";
  createdAt: string;
  deadline: string;
};

export const tickets: Ticket[] = [
  { id: 1842, subject: "Расчёт налога по договору поставки", customer: "ООО «Консалтинг-Волга»", author: "Иванов Алексей", responsible: "Екатерина Смирнова", status: "В работе", createdAt: "29.09.2026 09:42", deadline: "30.09.2026" },
  { id: 1841, subject: "Кадровое оформление совместителя", customer: "ООО «Вектор»", author: "Петрова Мария", responsible: "Анна Кузнецова", status: "Новая", createdAt: "29.09.2026 09:15", deadline: "01.10.2026" },
  { id: 1838, subject: "Проверка условий договора аренды", customer: "АО «Прогресс»", author: "Соколов Дмитрий", responsible: "Михаил Орлов", status: "Ожидает", createdAt: "28.09.2026 15:27", deadline: "30.09.2026" },
  { id: 1829, subject: "Изменения в учётной политике", customer: "ООО «Меридиан»", author: "Козлов Артём", responsible: "Екатерина Смирнова", status: "Закрыта", createdAt: "25.09.2026 11:04", deadline: "26.09.2026" },
];
