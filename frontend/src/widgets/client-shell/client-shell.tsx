"use client";
/* eslint-disable @next/next/no-img-element */
import { useState, useSyncExternalStore, type ReactNode } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { PortalProvider, usePortal } from "@/entities/client-portal/model/portal-provider";
import { PortalDesignContext, PortalLink, useDesignParam } from "@/shared/lib/client-design";
const asset = "/design/client/";
function subscribeMobile(callback: () => void) { const media = window.matchMedia("(max-width:700px)"); media.addEventListener("change", callback); return () => media.removeEventListener("change", callback); }
function mobileSnapshot() { return window.matchMedia("(max-width:700px)").matches; }
function serverSnapshot() { return false; }
const links = [["Главная", "/client", "d4c9d.svg"], ["Задать вопрос", "/client/question", "87799.svg"], ["Заявки", "/client/tickets", "3bfba.svg"], ["Мой календарь", "/client/calendar", "64f63.svg"], ["Библиотека консультаций", "/client/library", "269ff.svg"], ["О программе", "/client/about", "ea331.svg"]] as const;
export function ClientShell({ children }: { children: ReactNode }) { const design = useDesignParam(); return <PortalDesignContext.Provider value={design}><PortalProvider><ClientFrame>{children}</ClientFrame></PortalProvider></PortalDesignContext.Provider>; }
function ClientFrame({ children }: { children: ReactNode }) {
  const path = usePathname(); const design = useDesignParam(); const { data } = usePortal(); const [collapsed, setCollapsed] = useState(false); const [mobileOpen, setMobileOpen] = useState(false); const mobile = useSyncExternalStore(subscribeMobile, mobileSnapshot, serverSnapshot);
  if (path === "/client/login") return <div className={`portal portal--${design}`}>{children}</div>;
  return <div className={`portal portal--${design} ${collapsed ? "portal--collapsed" : ""} ${mobileOpen ? "portal--mobile-open" : ""}`}>
    <aside id="client-sidebar" className="portal-sidebar" aria-label="Кабинет клиента" inert={mobile && !mobileOpen}><PortalLink href="/client/profile" className="portal-identity"><span className={`portal-avatar ${data?.profile.avatarUrl ? "has-photo" : ""}`}><img src={data?.profile.avatarUrl ?? `${asset}abd88.svg`} width={36} height={46} alt="" /></span><strong>{data?.profile.name ?? "Граф Юлия Васильевна"}</strong></PortalLink><nav>{links.map(([title, href, icon]) => <PortalLink key={href} href={href} onClick={() => setMobileOpen(false)} className={`portal-nav-link ${path === href ? "is-active" : ""}`} aria-current={path === href ? "page" : undefined} title={title}><img src={asset + icon} alt="" width={16} height={16} /><span>{title}</span></PortalLink>)}</nav><footer><a href="tel:88005505690" className="portal-hotline"><img src={`${asset}cc5b5.png`} width={29} height={29} alt="" /><span>Горячая линия<br />8 800 550 5690</span></a><a href="https://консалтинг-волга.рф" target="_blank" rel="noreferrer">консалтинг-волга.рф</a></footer></aside>
    <button className="portal-backdrop" aria-label="Закрыть меню" onClick={() => setMobileOpen(false)} />
    <div className="portal-main"><header className="portal-topbar"><button className="portal-menu-toggle" onClick={() => { setCollapsed(v => !v); setMobileOpen(v => !v); }} aria-label="Переключить меню" aria-controls="client-sidebar" aria-expanded={mobile ? mobileOpen : !collapsed}><img src={`${asset}1e1d2.svg`} width={16} height={16} alt="" /></button><div className="portal-review-tools"><Link href="/screens">Все экраны</Link><label><span className="sr-only">Версия дизайна</span><select aria-label="Версия дизайна" value={design} onChange={e => { const url = new URL(window.location.href); url.searchParams.set("design", e.target.value); window.location.assign(url.pathname + url.search); }}><option value="original">Наша версия</option><option value="adapted">Адаптированная</option></select></label><Link href="/tickets">Кабинет сотрудника</Link></div><PortalLink className="portal-logout" href="/client/login"><img src={`${asset}c522b.svg`} width={16} height={16} alt="" />Выйти</PortalLink></header><main className="portal-content">{children}</main><p className="portal-demo-caption">Демонстрационные данные · API пока не подключён</p></div>
  </div>;
}
