"use client";
/* eslint-disable @next/next/no-img-element */
import { useRef, type ReactNode } from "react";
import { usePathname, useSearchParams } from "next/navigation";
import { usePortal } from "@/entities/client-portal/model/portal-provider";
import type { PortalAttachment, PortalTicket } from "@/entities/client-portal/model/types";
import { PortalLink } from "@/shared/lib/client-design";

export function PortalIcon({ file }: { file: string }) { return <span className="portal-icon"><img src={`/design/client/${file}`} alt="" /></span>; }
export function PortalBadge({ status }: { status: PortalTicket["status"] }) { return <span className={`portal-badge portal-badge--${status === "Требует уточнения" ? "red" : status === "Новая" ? "orange" : status === "В работе" ? "green" : "gray"}`}>{status}</span>; }
export function PortalTitle({ children, actions }: { children: ReactNode; actions?: ReactNode }) { return <header className="portal-page-title"><h1>{children}</h1>{actions}</header>; }
export function date(value: string) { return new Date(value).toLocaleDateString("ru-RU", { timeZone: "Europe/Moscow" }); }
export function dateTime(value: string) { return new Date(value).toLocaleString("ru-RU", { timeZone: "Europe/Moscow", dateStyle: "short", timeStyle: "short" }); }
const tabs = [["all", "Мои заявки", "19a0f.svg"], ["work", "В работе", "558a6.svg"], ["clarification", "Требуется уточнение", "cf927.svg"], ["favorites", "Избранное", "fb934.svg"], ["drafts", "Черновики", "25eb4.svg"], ["closed", "Закрытые", "03ca3.svg"]] as const;
export function PortalTicketLayout({ children }: { children: ReactNode }) {
  const { data } = usePortal(); const path = usePathname(); const tab = useSearchParams().get("tab") ?? "all";
  return <div className="portal-ticket-layout"><aside className="portal-ticket-nav">{path !== "/client/question" && <PortalLink className="primary-button portal-new-question" href="/client/question">Создать новую заявку</PortalLink>}<nav aria-label="Категории моих заявок">{tabs.map(([id, title, icon]) => <PortalLink key={id} href={`/client/tickets?tab=${id}`} aria-current={path === "/client/tickets" && tab === id ? "page" : undefined}><PortalIcon file={icon} />{title}{id === "clarification" && !!data?.tickets.filter(t => t.status === "Требует уточнения").length && <span className="portal-count">{data.tickets.filter(t => t.status === "Требует уточнения").length}</span>}</PortalLink>)}</nav><h3>АКТИВНЫЕ ПАКЕТЫ УСЛУГ</h3>{["Базовый", "Стандарт", "Проф", "Премиум", "Сайт", "IT", "Претензии", "Бухучет", "Иные услуги", "РГУ", "1С: ИТС"].map(title => { const active = data?.contracts.some(c => c.active && c.package === title); return <PortalLink className={`portal-package ${active ? "is-enabled" : ""}`} key={title} href="/client/profile"><span className={active ? "portal-dot is-green" : "portal-dot"} />{title}</PortalLink>; })}</aside><div className="portal-ticket-main">{children}</div></div>;
}
export function AttachmentPicker({ files, onChange }: { files: File[]; onChange: (files: File[]) => void }) {
  const input = useRef<HTMLInputElement>(null);
  return <><button type="button" className="secondary-button" onClick={() => input.current?.click()}><PortalIcon file="28cc9.svg" />Прикрепить файл</button><input ref={input} type="file" multiple hidden onChange={e => { onChange([...files, ...Array.from(e.target.files ?? [])]); e.target.value = ""; }} /></>;
}
export function AttachmentList({ attachments, files = [], onRemove }: { attachments: PortalAttachment[]; files?: File[]; onRemove?: (index: number) => void }) {
  const { repository } = usePortal();
  async function download(attachment: PortalAttachment, button: HTMLButtonElement) {
    try { const blob = await repository.downloadAttachment(attachment.id); const url = URL.createObjectURL(blob); const a = document.createElement("a"); a.href = url; a.download = attachment.name; a.click(); setTimeout(() => URL.revokeObjectURL(url), 1000); }
    catch (e) { const message = e instanceof Error ? e.message : "Не удалось скачать файл"; button.closest("section")?.querySelector("[data-attachment-error]")?.replaceChildren(message); }
  }
  if (!attachments.length && !files.length) return null;
  return <section className="portal-attachments" aria-label="Вложения"><ul>{attachments.map(file => <li key={file.id}><button type="button" onClick={e => download(file, e.currentTarget)}>{file.name}</button><span>{Math.ceil(file.size / 1024)} КБ</span></li>)}{files.map((file, i) => <li key={`${file.name}-${i}`}><span>{file.name}</span><span>{Math.ceil(file.size / 1024)} КБ</span>{onRemove && <button type="button" onClick={() => onRemove(i)} aria-label={`Убрать ${file.name}`}>×</button>}</li>)}</ul><p data-attachment-error role="status" /></section>;
}
