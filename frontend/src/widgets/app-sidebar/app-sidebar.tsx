"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { BookOpen } from "lucide-react";
import { navigation } from "@/shared/config/navigation";

export function AppSidebar() {
  const pathname = usePathname();
  return (
    <aside className="sidebar">
      <div className="sidebar-brand"><span className="sidebar-logo"><BookOpen size={20} /></span><div><strong>Библиотека</strong><small>консультаций</small></div></div>
      <nav className="sidebar-nav">
        {navigation.map(({ title, href, icon: Icon }) => {
          const active = href === "/" ? pathname === "/" : pathname.startsWith(href);
          return <Link key={href} href={href} className={`sidebar-link ${active ? "sidebar-link--active" : ""}`}><Icon size={17}/><span>{title}</span></Link>;
        })}
      </nav>
      <div className="sidebar-footer">Новая версия интерфейса</div>
    </aside>
  );
}
