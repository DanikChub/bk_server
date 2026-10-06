"use client";
import { usePathname } from "next/navigation";
import Link from "next/link";
import { useDemoTicketSession } from "@/shared/lib/access/demo-ticket-session";
import { demoUser } from "@/shared/config/demo-user";
import { DesignIcon } from "@/shared/ui/design-icon/design-icon";

export function AppHeader({ collapsed, onToggleMenu }: { collapsed: boolean; onToggleMenu: () => void }) {
  const pathname = usePathname() ?? "/tickets";
  const switchHref = pathname.startsWith("/employee/tickets") ? pathname.replace("/employee", "") : pathname.startsWith("/tickets") ? `/employee${pathname}` : "/employee/tickets";
  const { session, employee } = useDemoTicketSession();
  return <header className="topbar">
    <button className="sidebar-toggle" onClick={onToggleMenu} aria-label={collapsed ? "Открыть меню" : "Свернуть меню"} aria-controls="app-sidebar" aria-expanded={!collapsed}><DesignIcon name="collapse" /></button>
    <div className="topbar-actions">
      <Link className="demo-view-switch" href={switchHref} title="Переключить демонстрационный вариант">{employee ? "Вариант руководителя" : "Вариант сотрудника"}</Link>
      <span className="topbar-greeting">Здравствуйте, {session.displayName.split(" ").reverse().join(" ")}!</span>
      <button className="notification" disabled title="Уведомления будут подключены на этапе 5" aria-label={`Уведомления: ${demoUser.unreadNotifications} непрочитанных`}><DesignIcon name="bell" /><span>{demoUser.unreadNotifications}</span></button>
      <button className="logout-button" disabled title="Выход будет доступен после подключения авторизации"><DesignIcon name="logout" /><span>Выйти</span></button>
    </div>
  </header>;
}
