"use client";
import { usePathname } from "next/navigation";
import Link from "next/link";
import { useDemoTicketSession } from "@/shared/lib/access/demo-ticket-session";
import { useDemoNotifications } from "@/entities/notification/model/use-demo-notifications";
import { DesignIcon } from "@/shared/ui/design-icon/design-icon";

export function AppHeader({ collapsed, onToggleMenu }: { collapsed: boolean; onToggleMenu: () => void }) {
  const pathname = usePathname() ?? "/tickets";
  const switchHref = pathname.startsWith("/employee/") ? pathname.replace("/employee", "") : /^\/(tickets|notifications)(\/|$)/.test(pathname) ? `/employee${pathname}` : "/employee/tickets";
  const { session, employee } = useDemoTicketSession();
  const { counts } = useDemoNotifications(session.userId);
  return <header className="topbar">
    <button className="sidebar-toggle" onClick={onToggleMenu} aria-label={collapsed ? "Открыть меню" : "Свернуть меню"} aria-controls="app-sidebar" aria-expanded={!collapsed}><DesignIcon name="collapse" /></button>
    <div className="topbar-actions">
      <Link className="demo-view-switch" href={switchHref} title="Переключить демонстрационный вариант">{employee ? "Вариант руководителя" : "Вариант сотрудника"}</Link>
      <span className="topbar-greeting">Здравствуйте, {session.displayName.split(" ").reverse().join(" ")}!</span>
      <Link className="notification" href={employee ? "/employee/notifications" : "/notifications"} title="Мои уведомления" aria-label={`Уведомления: ${counts.unread} непрочитанных`}><DesignIcon name="bell" />{counts.unread > 0 && <span>{counts.unread}</span>}</Link>
      <button className="logout-button" disabled title="Выход будет доступен после подключения авторизации"><DesignIcon name="logout" /><span>Выйти</span></button>
    </div>
  </header>;
}
