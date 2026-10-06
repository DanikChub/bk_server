export type TicketStatus = "Новая" | "В работе" | "Возобновлённая" | "Закрыта" | "Требует уточнения";

export type Ticket = {
  id: number;
  subject: string;
  customer: string;
  author: string;
  responsible: string;
  responsibleId: string | null;
  status: TicketStatus;
  tag: string;
  type: string;
  section: string;
  createdAt: string;
  deadline: string;
};

export type Specialist = { id: string; name: string };
