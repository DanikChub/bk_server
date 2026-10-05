import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { CalendarClock, MessageSquare, UserRound } from "lucide-react";
import type { Ticket } from "@/entities/ticket/model/mock";

export function TicketDetailsPage({ ticket }: { ticket: Ticket }) {
 return <div><PageHeading title={`Заявка ${ticket.id}`} breadcrumbs={[{label: "Заявки", href: "/tickets"}, {label: `Заявка ${ticket.id}`}]} />
 <div className="detail-columns"><section className="panel"><h2>Информация об обращении</h2><div className="info-list"><div><span>Тема</span><strong>{ticket.subject}</strong></div><div><span>Клиент</span><strong>{ticket.customer}</strong></div><div><span>Создано</span><strong>{ticket.createdAt}</strong></div><div><span>Срок исполнения</span><strong>{ticket.deadline}</strong></div><div><span>Ответственный</span><strong>{ticket.responsible}</strong></div></div><div className="message-box"><MessageSquare size={20}/><div><strong>Описание обращения</strong><p>Клиент просит подготовить консультацию и проверить применимые нормы. Это демонстрационный текст до подключения API.</p></div></div></section>
 <aside className="panel side-panel"><h2>Автор</h2><div className="person"><div className="avatar large"><UserRound size={20}/></div><div><strong>{ticket.author}</strong><span>{ticket.customer}</span></div></div><div className="side-row"><CalendarClock size={17}/><div><span>Срок</span><strong>{ticket.deadline}</strong></div></div><button className="primary-button full">Взять в работу</button><button className="secondary-button full">Изменить ответственного</button></aside></div></div>
}
