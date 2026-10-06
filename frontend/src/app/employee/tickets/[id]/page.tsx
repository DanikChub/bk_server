import { notFound } from "next/navigation";
import { tickets } from "@/entities/ticket/model/mock";
import { TicketDetailsPage } from "@/views/tickets/ui/ticket-details-page";

export function generateStaticParams() {
  return tickets.map(({ id }) => ({ id: String(id) }));
}

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const ticket = tickets.find((item) => item.id === Number(id));
  if (!ticket) notFound();
  return <TicketDetailsPage ticket={ticket} />;
}
