"use client";

import Link from "next/link";
import { ArrowLeft, Plus, Search } from "lucide-react";
import type { Customer } from "@/entities/customer/model/mock";
import { tickets } from "@/entities/ticket/model/mock";
import { DataTable } from "@/shared/ui/data-table/data-table";
import { StatusBadge } from "@/shared/ui/status-badge/status-badge";

export function CustomerDetailsPage({ customer }: { customer: Customer }) {
  return <div><div className="breadcrumbs"><Link href="/customers">Клиенты</Link><span>/</span><span>{customer.name}</span></div>
    <section className="panel customer-card"><Link href="/customers" className="back-title"><ArrowLeft size={18}/>{customer.name}</Link><div className="details-grid"><div><span>Статус</span><StatusBadge tone={customer.active?"green":"gray"}>{customer.active?"Активен":"Неактивен"}</StatusBadge></div><div><span>Дата создания</span><strong>{customer.createdAt}</strong></div><div><span>Контактное лицо</span><strong>{customer.contact}</strong></div><div><span>Ответственный</span><strong>{customer.responsible}</strong></div></div>
    <div className="tabs"><button className="active">Заявки клиента</button><button>Пользователи</button><button>Статистика</button><button>Договоры</button><button>Журнал событий</button></div>
    <div className="subheading"><h2>Все заявки</h2><div className="inline-search"><Search size={15}/><input placeholder="Поиск"/><button className="primary-button">Найти</button></div></div>
    <button className="primary-button compact"><Plus size={15}/>Создать заявку</button>
    <DataTable rows={tickets.slice(0,3)} href={t=>`/tickets/${t.id}`} columns={[{title:"№",render:t=>`#${t.id}`},{title:"Тема",render:t=>t.subject},{title:"Ответственный",render:t=>t.responsible},{title:"Статус",render:t=><StatusBadge tone={t.status==="Закрыта"?"gray":"blue"}>{t.status}</StatusBadge>},{title:"Срок",render:t=>t.deadline}]}/></section>
  </div>;
}
