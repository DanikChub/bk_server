"use client";
import { useState } from "react";
import { useSearchParams } from "next/navigation";
import { PortalState, usePortal } from "@/entities/client-portal/model/portal-provider";
import { PortalLink } from "@/shared/lib/client-design";
export function ClientLibraryPage() {
  const { data } = usePortal(); const params = useSearchParams(); const [query, setQuery] = useState(""); const id = params.get("article");
  const rows = [...(params.get("category") === "news" ? [] : data?.content.articles ?? []), ...(data?.content.news ?? [])].filter(item => (!id || item.id === id) && item.title.toLowerCase().includes(query.toLowerCase()));
  return <PortalState><section className="portal-library-page"><h1>{params.get("category") === "news" ? "Новости" : "Библиотека консультаций"}</h1><p>Библиотека консультаций: <a className="portal-accent-link" href={process.env.NEXT_PUBLIC_DATA_MODE==="demo"?"https://bk.kv34.ru/":"/api/library"} target="_blank" rel="noreferrer">bk.kv34.ru</a>.</p>{data?.contentError && <p role="status">{data.contentError}</p>}{id ? <PortalLink href="/client/library" className="portal-accent-link">← Все материалы</PortalLink> : <label className="portal-search"><span className="sr-only">Поиск материалов</span><input value={query} onChange={e => setQuery(e.target.value)} placeholder="Поиск материалов" /></label>}{rows.map(item => <article key={item.id}><h2>{id ? item.title : <PortalLink href={`/client/library?article=${item.id}`}>{item.title}</PortalLink>}</h2>{id && <p>{item.body}</p>}</article>)}{!rows.length && <p className="portal-empty">Материалы не найдены.</p>}</section></PortalState>;
}
