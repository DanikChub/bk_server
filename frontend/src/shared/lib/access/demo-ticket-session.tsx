"use client";
import { createContext, useContext } from "react";
import { managerTicketSession } from "@/shared/config/demo-ticket-sessions";
import type { TicketSession } from "./ticket-access";

export const DemoTicketSessionContext = createContext<{ session: TicketSession; employee: boolean; ticketsHref: string }>({
  session: managerTicketSession, employee: false, ticketsHref: "/tickets",
});
export function useDemoTicketSession() { return useContext(DemoTicketSessionContext); }
