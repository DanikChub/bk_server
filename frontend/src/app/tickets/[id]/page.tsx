import { notFound } from "next/navigation";
import { tickets } from "@/entities/ticket/model/mock";
import { TicketDetailsPage } from "@/views/tickets/ui/ticket-details-page";

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
 const { id } = await params;
 const ticket = tickets.find(t => t.id === Number(id));
 if (!ticket) notFound();
 return <TicketDetailsPage ticket={ticket}/>;
}
