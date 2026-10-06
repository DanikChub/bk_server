"use client";
/* eslint-disable @next/next/no-img-element */

import { UserRound } from "lucide-react";
import { useDemoTicketSession } from "@/shared/lib/access/demo-ticket-session";
import { hasPolicy } from "@/shared/lib/access/ticket-access";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { administrationNavigation, navigation } from "@/shared/config/navigation";
import { demoUser } from "@/shared/config/demo-user";
import { DesignIcon } from "@/shared/ui/design-icon/design-icon";

export function AppSidebar({ collapsed, hidden, onNavigate }: { collapsed: boolean; hidden: boolean; onNavigate: () => void }) {
  const { session, employee, ticketsHref } = useDemoTicketSession();
  const showAdministration = hasPolicy(session, "TicketSection") || hasPolicy(session, "ContractStatuses");
  const pathname = usePathname() ?? "/";
  const adminActive = administrationNavigation.some(item => pathname.startsWith(item.href));
  return (
    <aside className="sidebar" aria-label="Основная навигация" id="app-sidebar" inert={hidden}>
      <div className="sidebar-profile">
        {employee ? <span className="sidebar-demo-avatar"><UserRound size={30} /></span> : <img src={demoUser.avatar} width={48} height={48} alt="" />}
        <strong>{employee ? <>Макаровский<br />Вадим</> : <>{demoUser.lastName}<br />{demoUser.firstName} {demoUser.patronymic}</>}</strong>
      </div>
      <nav className="sidebar-nav">
        {navigation.map(({ title, href, asset, available }) => {
          if (employee && href !== "/tickets" && href !== "/notifications") return null;
          const target = href === "/tickets" ? ticketsHref : href === "/notifications" && employee ? "/employee/notifications" : href;
          const active = target === "/" ? pathname === "/" : pathname.startsWith(target);
          const content = <><DesignIcon name={asset} /><span>{title}</span></>;
          return available ? <Link key={href} href={target} title={collapsed ? title : undefined} aria-current={active ? "page" : undefined} onClick={onNavigate} className={`sidebar-link ${active ? "sidebar-link--active" : ""}`}>{content}</Link>
            : <button key={href} className="sidebar-link" disabled title="Раздел ещё не перенесён">{content}</button>;
        })}
        {showAdministration && <details className={`sidebar-administration ${adminActive ? "sidebar-administration--active" : ""}`} open={adminActive || undefined}>
          <summary className="sidebar-link" title={collapsed ? "Администрирование" : undefined}><DesignIcon name="administration" /><span>Администрирование</span><span className="sidebar-chevron">‹</span></summary>
          <div className="sidebar-submenu">{administrationNavigation.map(item => item.available ? <Link key={item.href} href={item.href} onClick={onNavigate} aria-current={pathname.startsWith(item.href) ? "page" : undefined}>{item.title}</Link> : <button key={item.href} disabled title="Раздел ещё не перенесён">{item.title}</button>)}</div>
        </details>}
        <a className="sidebar-link" href="https://bk.kv34.ru/" target="_blank" rel="noreferrer" title={collapsed ? "Библиотека консультаций" : undefined}><DesignIcon name="library" /><span>Библиотека<br />консультаций</span></a>
        <button className="sidebar-link" disabled title="Раздел ещё не перенесён"><DesignIcon name="about" /><span>О программе</span></button>
      </nav>
      <div className="sidebar-footer"><a className="sidebar-hotline" href="tel:88005505690"><DesignIcon name="hotline" /><span>Горячая линия<br />8 800 550 5690</span></a><a className="sidebar-website" href="https://консалтинг-волга.рф" target="_blank" rel="noreferrer">консалтинг-волга.рф</a></div>
    </aside>
  );
}
