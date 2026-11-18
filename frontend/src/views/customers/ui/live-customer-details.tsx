"use client";
import Link from 'next/link';
import { useState } from 'react';
import { useApi } from '@/views/backend/ui/live-staff';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';
import './customer-profile.css';
import {NewDesignIcon} from '@/shared/ui/design-icon/new-design-icon';
type Row=Record<string,unknown>;
type Page={items:Row[];totalCount:number};
const text=(v:unknown)=>String(v??'');
const date=(v:unknown)=>v?new Date(text(v)).toLocaleDateString('ru-RU',{timeZone:'Europe/Moscow'}):'—';
const fullName=(v:Row)=>[v.LastName,v.FirstName,v.MiddleName].filter(Boolean).join(' ');
type Editor={company:Row;contact:{fullName:string;jobPost:string}|null;meta:{networkEdition:string;subordinateIds:string[];bitrixUrl:string}};
function safeUrl(value:unknown){try{const url=new URL(String(value));return ['https:','http:'].includes(url.protocol)?url.href:null;}catch{return null;}}
export function LiveCustomerDetails({id}:{id:string}){
  const {data:c,error}=useApi<Row>('customers/'+id),editor=useApi<Editor>(`customers/${id}/edit-form`),managers=useApi<Row[]>('specialists'),packages=useApi<Page>(`customers/${id}/packages`);
  const [tab,setTab]=useState('tickets');
  if(error)return <section className="panel" role="alert">{error}</section>;
  if(!c)return <section className="panel" role="status">Загрузка клиента…</section>;
  const responsible=managers.data?.find(m=>m.Id===c.ResponsibleManagerId),meta=editor.data?.meta;
  const edit='/customers/'+id+'/edit';
  return <div className="customer-profile"><PageHeading title="Профиль клиента" breadcrumbs={[{label:'Клиенты',href:'/customers'},{label:text(c.ShortName)}]}/>
    <div className="customer-profile-layout"><div><header className="customer-profile-heading"><h2>{text(c.LongName)}</h2><Link href={edit} aria-label="Редактировать клиента" title="Редактировать клиента"><NewDesignIcon file="642cb.svg"/></Link></header>
      <div className="customer-profile-summary"><dl><div><dt>Статус:</dt><dd><span className={c.IsActive?'customer-state customer-state--active':'customer-state'} aria-label={c.IsActive?'Активный клиент':'Неактивный клиент'}>{c.IsActive?'✓':'×'}</span></dd></div><div><dt>Время:</dt><dd>—</dd></div></dl>
        <dl><div><dt>Дата создания:</dt><dd>{date(c.CreationTime)}</dd></div><div><dt>Ссылка на Битрикс:</dt><dd>{safeUrl(meta?.bitrixUrl)?<a className="customer-text-link" target="_blank" rel="noreferrer" href={safeUrl(meta?.bitrixUrl)!}>{meta?.bitrixUrl}</a>:<Link href={edit}>Добавить ссылку</Link>}</dd></div></dl>
        <dl><div><dt>Контактное лицо:</dt><dd>{editor.data?.contact?.fullName||'Не указано'}{editor.data?.contact?.jobPost&&<small>{editor.data.contact.jobPost}</small>}</dd><Link href={edit} aria-label="Изменить контактное лицо"><NewDesignIcon file="13589.svg"/></Link></div><div><dt>Ответственный:</dt><dd>{responsible?fullName(responsible):'Не назначен'}</dd><Link href={edit} aria-label="Изменить ответственного"><NewDesignIcon file="13589.svg"/></Link></div></dl>
      </div>
      <section className="panel customer-profile-content"><nav className="customer-profile-tabs" aria-label="Разделы клиента">{[['tickets','Заявки клиента'],['users','Пользователи'],['statistics','Статистика'],['contracts','Договоры'],['events','Журнал событий']].map(([key,title])=><button key={key} aria-current={tab===key?'page':undefined} onClick={()=>setTab(key)}>{title}</button>)}</nav><CustomerTab key={id+tab} id={id} tab={tab}/></section>
    </div><aside className="customer-profile-sidebar"><p>ИНН / КПП: {text(c.INNNumber)||'—'} / {text(c.KPPNumber)||'—'}</p><p>Адрес: {text(c.Address)||'Не указан'}</p><p>Телефон: {text(c.PhoneNumber)||'Не указан'}</p><p>Email: {c.Email?<a href={'mailto:'+text(c.Email)}>{text(c.Email)}</a>:'Не указан'}</p><p>Сайт: {safeUrl(c.SiteUrl)?<a target="_blank" rel="noreferrer" href={safeUrl(c.SiteUrl)!}>{text(c.SiteUrl)}</a>:'Не указан'}</p><p>СИ: {meta?.networkEdition||'Не указано'}</p><p>Примечание: {text(c.CommentNotes)||'Не указано'}</p><div className="customer-profile-packages">Пакеты услуг: {packages.data?.items.map(p=><span key={text(p.Id)}>{text(p.Name)}</span>)}</div><p>Подведы:</p>{meta?.subordinateIds.length?<Subordinates ids={meta.subordinateIds}/>:<span>Не указаны</span>}<Link href={edit} className="secondary-button customer-profile-edit">Редактировать клиента</Link></aside></div>
  </div>;
}
function Subordinates({ids}:{ids:string[]}){const {data}=useApi<Page>('customers?take=200');return <ul>{ids.map(id=><li key={id}><Link href={'/customers/'+id}>{text(data?.items.find(c=>c.Id===id)?.ShortName)||'Открыть организацию'}</Link></li>)}</ul>;}
function CustomerTab({id,tab}:{id:string;tab:string}){
  const [search,setSearch]=useState(''),[query,setQuery]=useState(''),[page,setPage]=useState(0),[take,setTake]=useState(10);
  const q=new URLSearchParams({tenantId:id,search:query,take:String(take),skip:String(page*take)});
  const path=tab==='tickets'?'tickets?'+q:tab==='contracts'?'contracts?'+q:tab==='users'?`customers/${id}/users`:tab==='statistics'?`customers/${id}/statistics`:`customers/${id}/events?`+q;
  const {data,error}=useApi<Page|Row[]>(path);
  if(error)return <p role="alert">{error}</p>;
  if(!data)return <p role="status">Загрузка…</p>;
  if(tab==='statistics')return <CustomerStatistics data={data as unknown as {constraints:Row[];tickets:Row[]}}/>;
  const list=Array.isArray(data)?data:data.items,total=Array.isArray(data)?list.length:data.totalCount;
  const filtered=Array.isArray(data)&&query?list.filter(row=>Object.values(row).join(' ').toLowerCase().includes(query.toLowerCase())):list;
  const items=Array.isArray(data)?filtered.slice(page*take,(page+1)*take):list;
  const columns:Record<string,[string,string][]>={tickets:[['StatusDisplayName','Статус'],['Id','Номер заявки'],['Subject','Тема'],['CreationTime','Дата создания'],['LastModificationTime','Дата изменения']],users:[['LastName','Фамилия'],['FirstName','Имя'],['JobPost','Должность'],['Email','E-mail'],['UserName','Логин']],contracts:[['Name','Договор'],['ContractStartDate','Дата начала'],['ContractFinishDate','Дата окончания']],events:[['CreatorLastName','Сотрудник'],['Action','Действие'],['CreationTime','Дата']]};
  return <><div className="customer-profile-tabheading"><h3>{tab==='tickets'?'Все заявки':tab==='users'?'Пользователи':tab==='contracts'?'Договоры':'Журнал событий'} ({total})</h3><form className="inline-search" onSubmit={e=>{e.preventDefault();setQuery(search);setPage(0);}}><input aria-label="Поиск в кабинете клиента" placeholder="⌕ Поиск" value={search} onChange={e=>setSearch(e.target.value)}/><button className="primary-button">Найти</button></form></div><div className="table-scroll"><table className="data-table"><thead><tr>{columns[tab].map(([key,title])=><th key={key}>{title}</th>)}</tr></thead><tbody>{items.map(row=><tr key={text(row.Id)}>{columns[tab].map(([key])=><td key={key}>{tab==='tickets'&&key==='Id'?<Link href={'/tickets/'+row.Id}>{text(row.Id)}</Link>:tab==='contracts'&&key==='Name'?<Link href={'/contracts/'+row.Id}>{text(row.Name)}</Link>:key.includes('Time')||key.includes('Date')?date(row[key]):text(row[key])||'—'}</td>)}</tr>)}{!items.length&&<tr><td colSpan={columns[tab].length}>Записи не найдены</td></tr>}</tbody></table></div><footer className="customer-profile-pagination"><label>Показать <select value={take} onChange={e=>{setTake(Number(e.target.value));setPage(0);}}>{[10,30,50].map(n=><option key={n}>{n}</option>)}</select> записей</label><div><button disabled={!page} aria-label="Предыдущая страница" onClick={()=>setPage(p=>p-1)}>‹</button><span>{page+1}</span><button disabled={(page+1)*take>=total} aria-label="Следующая страница" onClick={()=>setPage(p=>p+1)}>›</button></div></footer></>;
}
function CustomerStatistics({data}:{data:{constraints:Row[];tickets:Row[]}}){return <div className="customer-statistics"><section><h3>Заявки по статусам</h3><dl>{data.tickets?.map(t=><div key={text(t.Name)}><dt>{text(t.Name)}</dt><dd>{text(t.count)}</dd></div>)}</dl>{!data.tickets?.length&&<p>Заявок пока нет.</p>}</section><section><h3>Использование ограничений</h3><dl>{data.constraints?.map((c,i)=><div key={text(c.Id)||String(i)}><dt>{text(c.Month)}.{text(c.Year)}</dt><dd>{text(c.Sum)} / {text(c.MaxCount)}</dd></div>)}</dl>{!data.constraints?.length&&<p>Ограничения пока не назначены.</p>}</section></div>;}
