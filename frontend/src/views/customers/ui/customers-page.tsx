"use client";

import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { useMemo, useState } from "react";
import { Download, Filter, Plus, Search } from "lucide-react";
import { customers } from "@/entities/customer/model/mock";
import { DataTable } from "@/shared/ui/data-table/data-table";
import { StatusBadge } from "@/shared/ui/status-badge/status-badge";

export function CustomersPage() {
  const [query, setQuery] = useState("");
  const rows = useMemo(() => customers.filter(c => [c.name,c.inn,c.contact,c.responsible].join(" ").toLowerCase().includes(query.toLowerCase())), [query]);
  return (
    <div><PageHeading title={`Клиенты (${customers.length})`} breadcrumbs={[{ label: "Клиенты" }]} />
      <section className="panel">
        <div className="toolbar"><div className="toolbar-group"><button className="secondary-button"><Download size={15}/>Открыть в Excel</button><button className="primary-button"><Plus size={15}/>Новый клиент</button><button className="primary-button primary-button--muted">Мои клиенты</button></div><div className="search-box"><button className="icon-button"><Filter size={16}/></button><div><Search size={15}/><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Поиск"/></div><button className="primary-button">Найти</button></div></div>
        <DataTable rows={rows} href={c=>`/customers/${c.id}`} columns={[
          {title:"Клиент",render:c=>c.name},{title:"ИНН",render:c=>c.inn},{title:"Контактное лицо",render:c=>c.contact},{title:"Ответственный",render:c=>c.responsible},{title:"Создан",render:c=>c.createdAt},{title:"Статус",render:c=><StatusBadge tone={c.active?"green":"gray"}>{c.active?"Активен":"Неактивен"}</StatusBadge>}
        ]}/>
        <div className="table-footer"><span>Показано {rows.length} из {customers.length}</span><div><button>‹</button><button className="active">1</button><button>2</button><button>›</button></div></div>
      </section>
    </div>
  );
}
