"use client";
import { useState, useSyncExternalStore, type ReactNode } from "react";
import { usePathname } from "next/navigation";
import { DemoTicketSessionContext } from "@/shared/lib/access/demo-ticket-session";
import { employeeTicketSession, managerTicketSession } from "@/shared/config/demo-ticket-sessions";
import { AppHeader } from "@/widgets/app-header/app-header";
import { AppSidebar } from "@/widgets/app-sidebar/app-sidebar";

const mobileQuery = "(max-width: 700px)";
function subscribeMobile(callback: () => void) {
  const query = window.matchMedia(mobileQuery);
  query.addEventListener("change", callback);
  return () => query.removeEventListener("change", callback);
}
function getMobileSnapshot() { return window.matchMedia(mobileQuery).matches; }
function getServerSnapshot() { return false; }

export function AppShell({ children }: { children: ReactNode }) {
  const employee = (usePathname() ?? "").startsWith("/employee/");
  const session = employee ? employeeTicketSession : managerTicketSession;
  const ticketsHref = employee ? "/employee/tickets" : "/tickets";
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const mobile = useSyncExternalStore(subscribeMobile, getMobileSnapshot, getServerSnapshot);
  const menuCollapsed = mobile ? !mobileOpen : collapsed;
  function toggleMenu() {
    if (mobile) setMobileOpen(value => !value);
    else setCollapsed(value => !value);
  }
  return <DemoTicketSessionContext.Provider key={session.userId} value={{ session, employee, ticketsHref }}><div className={`app-shell ${collapsed ? "app-shell--collapsed" : ""} ${mobileOpen ? "app-shell--mobile-open" : ""}`}>
    <AppSidebar hidden={mobile && !mobileOpen} collapsed={collapsed} onNavigate={() => setMobileOpen(false)} />
    <button className="sidebar-backdrop" aria-label="Закрыть меню" onClick={() => setMobileOpen(false)} />
    <div className="app-main"><AppHeader collapsed={menuCollapsed} onToggleMenu={toggleMenu} /><main className="page-content">{children}</main></div>
  </div></DemoTicketSessionContext.Provider>;
}
