import type { Specialist, Ticket } from "./types";
export type { Ticket } from "./types";

// Demo data only: no API or authenticated session is connected yet.
export const demoSpecialistId = "morozov";
export const specialists: Specialist[] = [
  { id: "morozov", name: "Морозов Иван" },
  { id: "makarovsky", name: "Макаровский Вадим" },
  { id: "semerkova", name: "Семеркова Диана" },
  { id: "ivonyak", name: "Ивоняк Александр" },
  { id: "smirnova", name: "Смирнова Екатерина" },
  { id: "kuznetsova", name: "Кузнецова Анна" },
  { id: "orlov", name: "Орлов Михаил" },
  { id: "vyskub", name: "Выскуб" },
  { id: "drabinin", name: "Драбинин" },
  { id: "ermilov", name: "Ермилов" },
  { id: "boychuk", name: "Бойчук" },
  { id: "bortnikov", name: "Бортников" },
  { id: "brykalina", name: "Брыкалина" },
];

const customers = [
  "Адм Мирновского сп Симферопольского района",
  "Адм Дьячкинского сп Ростовской области",
  "Линёвский Дом культуры Жирновского района Волгоградской области, МУ",
  "Адм Красногорского сп Ленинского р-на РК",
  "МУП Михайловское ВКХ Волгоградская область",
  "Адм Дубовского сп Пролетарского района Ростовской области",
  "Местная администрация с.п. Малакановское Прохладненского района КБР",
  "Адм Новохопёрского сп Белогорского района РК",
];
const sections = ["Трудовое законодательство", "Гражданское законодательство", "Закупки (44-ФЗ, 223-ФЗ)", "Земельное законодательство"];
const types = ["консалтинг", "претензия", "размещение", "разработка", "сайт", "IT"];
const statuses: Ticket["status"][] = ["В работе", "Новая", "Возобновлённая", "Закрыта"];

export const tickets: Ticket[] = [
  { id: 1842, subject: "Расчёт налога по договору поставки", customer: "ООО «Консалтинг-Волга»", author: "Иванов Алексей", responsible: "Смирнова Екатерина", responsibleId: "smirnova", status: "В работе", tag: "срочно", type: "консалтинг", section: "Налоговое законодательство", createdAt: "29.09.2026 09:42", deadline: "25.10.2026" },
  { id: 1841, subject: "Кадровое оформление совместителя", customer: "ООО «Вектор»", author: "Петрова Мария", responsible: "", responsibleId: null, status: "Новая", tag: "", type: "консалтинг", section: "Трудовое законодательство", createdAt: "29.09.2026 09:15", deadline: "25.10.2026" },
  { id: 1838, subject: "Проверка условий договора аренды", customer: "АО «Прогресс»", author: "Соколов Дмитрий", responsible: "Орлов Михаил", responsibleId: "orlov", status: "Возобновлённая", tag: "VIP", type: "претензия", section: "Муниципальное имущество", createdAt: "28.09.2026 15:27", deadline: "25.10.2026" },
  { id: 1829, subject: "Изменения в учётной политике", customer: "ООО «Меридиан»", author: "Козлов Артём", responsible: "Морозов Иван", responsibleId: demoSpecialistId, status: "Закрыта", tag: "", type: "консалтинг", section: "Бухгалтерский учет", createdAt: "25.09.2026 11:04", deadline: "26.09.2026" },
  ...Array.from({ length: 36 }, (_, index): Ticket => {
    const specialist = index < 16 ? specialists[0] : specialists[1 + index % 7];
    const status = index < 10 ? "В работе" : statuses[index % statuses.length];
    return {
      id: 128569 - index * 7,
      subject: "Консультация: " + sections[index % sections.length],
      customer: customers[index % customers.length],
      author: "Смирнова Виктория",
      responsible: status === "Новая" ? "" : specialist.name,
      responsibleId: status === "Новая" ? null : specialist.id,
      status,
      tag: ["срочно", "", "", "", "под ключ", "", "VIP", ""][index % 8],
      type: types[index % types.length],
      section: sections[index % sections.length],
      createdAt: `${String(30 - index % 25).padStart(2, "0")}.09.2026 15:34`,
      deadline: index === 4 ? "01.10.2026" : "25.10.2026",
    };
  }),
];

export const initialFavoriteTicketIds = [128569, 128541, 1838];
