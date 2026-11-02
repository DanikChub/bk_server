"use client";
/* eslint-disable @next/next/no-img-element */
import { PortalState, usePortal } from "@/entities/client-portal/model/portal-provider";
import { PortalLink, usePortalDesign } from "@/shared/lib/client-design";

export function ClientHomePage() {
  const { data } = usePortal(); const adapted = usePortalDesign() === "adapted";
  return <PortalState>{data && <div className="portal-home" data-figma-node={adapted ? "2:6167" : "1:733"}>
    <div className="portal-home-left"><PortalLink href="/client/library" className="portal-banner portal-banner--social"><img src="/design/client/4f6f2.png" alt="ИПС Консалтинг — мы в социальных сетях" /></PortalLink>
      <div className="portal-app-tiles"><PortalLink href="/client/library?category=calculators">Калькуляторы</PortalLink><PortalLink href="/client/library?category=classifiers">Классификаторы</PortalLink>{adapted && <PortalLink href="/client/library?category=references">Справки</PortalLink>}<PortalLink href="/client/calendar">Мой календарь</PortalLink></div>
      <section className="portal-news"><h2>Главное:</h2><div><ul>{data.content.news.map(news => <li key={news.id}><PortalLink href={`/client/library?article=${news.id}`}>{news.title}</PortalLink></li>)}</ul><PortalLink href="/client/library?category=news" className="portal-muted-link">Читать все новости</PortalLink></div></section>
    </div><div className="portal-home-right"><PortalLink href="/client/question" className="portal-banner portal-banner--service"><img src="/design/client/026b7.png" alt="Помощь специалистов ИПС Консалтинг" /></PortalLink><section className="portal-library-preview"><h2>Библиотека консультаций:</h2><ul>{data.content.articles.map(article => <li key={article.id}><img src={`/design/client/${article.icon}`} width={57} height={59} alt="" /><PortalLink href={`/client/library?article=${article.id}`}>{article.title}</PortalLink></li>)}</ul></section></div>
  </div>}</PortalState>;
}
