export const navigation = [
  { title: "Главная", href: "/", asset: "home", available: true },
  { title: "Заявки", href: "/tickets", asset: "tickets", available: true },
  { title: "Уведомления", href: "/notifications", asset: "bell", available: true },
  { title: "Клиенты", href: "/customers", asset: "customers", available: true },
  { title: "Договоры", href: "/contracts", asset: "contracts", available: true },
  { title: "Пользователи", href: "/users", asset: "customers", available: true },
] as const;

export const administrationNavigation = [
  { title: "Сотрудники", href: "/employees", available: true },
  { title: "Шаблоны ответов", href: "/answer-templates", available: true },
  { title: "Разделы заявок", href: "/ticket-sections", available: true },
  { title: "Пакеты услуг", href: "/service-packages", available: true },
] as const;
