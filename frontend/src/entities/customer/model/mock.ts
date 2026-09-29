export type Customer = {
  id: string;
  name: string;
  inn: string;
  contact: string;
  responsible: string;
  createdAt: string;
  active: boolean;
};

export const customers: Customer[] = [
  { id: "volga", name: "ООО «Консалтинг-Волга»", inn: "3444251027", contact: "Иванов Алексей Сергеевич", responsible: "Екатерина Смирнова", createdAt: "12.09.2026", active: true },
  { id: "vector", name: "ООО «Вектор»", inn: "3662284102", contact: "Петрова Мария Андреевна", responsible: "Анна Кузнецова", createdAt: "08.09.2026", active: true },
  { id: "progress", name: "АО «Прогресс»", inn: "3665147820", contact: "Соколов Дмитрий Игоревич", responsible: "Екатерина Смирнова", createdAt: "02.09.2026", active: true },
  { id: "atlas", name: "ООО «Атлас»", inn: "3662219504", contact: "Волкова Ольга Павловна", responsible: "Михаил Орлов", createdAt: "21.08.2026", active: false },
  { id: "meridian", name: "ООО «Меридиан»", inn: "3664098172", contact: "Козлов Артём Олегович", responsible: "Анна Кузнецова", createdAt: "14.08.2026", active: true },
];
