"use client";
import { useStoredValue } from "@/shared/lib/use-stored-value";
import { isDetailMap, type TicketDetail } from "./detail";
import { tickets } from "./mock";
const initial: Record<string,TicketDetail>={};
export function useDemoTicketStore(){
  const [details,save]=useStoredValue("tickets:demo:details:v1",initial,isDetailMap);
  const rows=tickets.filter(ticket=>!details[ticket.id]?.deleted).map(ticket=>details[ticket.id]?.ticket??ticket);
  return {details,rows,saveDetail:(detail:TicketDetail)=>save({...details,[detail.ticket.id]:detail})};
}
