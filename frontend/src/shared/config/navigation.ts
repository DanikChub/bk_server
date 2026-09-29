import { BriefcaseBusiness, Building2, FileText, Home, Inbox, Ticket, Users } from "lucide-react";

export const navigation = [
  { title: "Главная", href: "/", icon: Home },
  { title: "Клиенты", href: "/customers", icon: Building2 },
  { title: "Сотрудники", href: "/employees", icon: Users },
  { title: "Обращения", href: "/tickets", icon: Ticket },
  { title: "Договоры", href: "/contracts", icon: FileText },
  { title: "Шаблоны ответов", href: "/answer-templates", icon: BriefcaseBusiness },
  { title: "Уведомления", href: "/inbox", icon: Inbox },
] as const;
