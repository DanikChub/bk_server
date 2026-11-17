import { notFound } from "next/navigation";
import { tickets } from "@/entities/ticket/model/mock";
import { TicketDetailsPage } from "@/views/tickets/ui/ticket-details-page";
import { LiveTicketDetails } from '@/views/backend/ui/live-staff';

export function generateStaticParams() {
  return process.env.NEXT_PUBLIC_DATA_MODE==='demo'?tickets.map(({ id }) => ({ id: String(id) })):[];
}

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  if(process.env.NEXT_PUBLIC_DATA_MODE!=='demo')return <LiveTicketDetails id={id}/>;
  const ticket = tickets.find((item) => item.id === Number(id));
  if (!ticket) notFound();
  return <TicketDetailsPage ticket={ticket} />;
}
