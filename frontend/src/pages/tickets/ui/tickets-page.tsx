"use client";

import { useMemo, useState } from "react";
import { Filter, Plus, Search, SlidersHorizontal } from "lucide-react";
import { tickets } from "@/entities/ticket/model/mock";
import { DataTable } from "@/shared/ui/data-table/data-table";
import { StatusBadge } from "@/shared/ui/status-badge/status-badge";

const tone = (status: string) => status === "Закрыта" ? "gray" : status === "Новая" ? "green" : status === "Ожидает" ? "orange" : "blue";

export function TicketsPage() {
 const [query,setQuery]=useState("");
 const rows=useMemo(()=>tickets.filter(t=>Object.values(t).join(" ").toLowerCase().includes(query.toLowerCase())),[query]);
 return <div><div className="page-heading"><div><h1>Обращения <span>{tickets.length}</span></h1><p>Заявки клиентов и работа специалистов</p></div><button className="primary-button"><Plus size={16}/>Новое обращение</button></div>
 <div className="ticket-layout"><aside className="ticket-summary panel"><h3>Мои обращения</h3><ul><li>Все <strong>24</strong></li><li>Новые <strong>5</strong></li><li>В работе <strong>11</strong></li><li>Ожидают <strong>4</strong></li><li>Просрочены <strong>2</strong></li></ul><hr/><ul className="muted"><li>Я автор</li><li>Я ответственный</li><li>Без ответственного</li></ul></aside>
 <section className="panel ticket-table"><div className="tabs"><button className="active">Все</button><button>Новые</button><button>В работе</button><button>Закрытые</button></div><div className="toolbar"><div className="toolbar-group"><button className="secondary-button"><SlidersHorizontal size={15}/>Колонки</button><button className="secondary-button"><Filter size={15}/>Фильтры</button></div><div className="search-box"><div><Search size={15}/><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Поиск"/></div><button className="primary-button">Найти</button></div></div>
 <DataTable rows={rows} href={t=>`/tickets/${t.id}`} columns={[{title:"№",render:t=>`#${t.id}`},{title:"Тема",render:t=>t.subject},{title:"Клиент",render:t=>t.customer},{title:"Ответственный",render:t=>t.responsible},{title:"Статус",render:t=><StatusBadge tone={tone(t.status)}>{t.status}</StatusBadge>},{title:"Срок",render:t=>t.deadline}]}/></section></div></div>
}
