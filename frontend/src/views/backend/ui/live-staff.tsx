"use client";
import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { backend } from '@/entities/client-portal/api/http-repository';
import '@/widgets/tickets-list/ui/tickets-list.css';
import '@/widgets/ticket-details/ui/ticket-details.css';
import '@/widgets/notifications/ui/notifications.css';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';

type Row=Record<string,unknown>;
type Page={items:Row[];totalCount:number};
const text=(value:unknown)=>String(value??'');
const date=(value:unknown)=>value?new Date(text(value)).toLocaleString('ru-RU',{timeZone:'Europe/Moscow',dateStyle:'short',timeStyle:'short'}):'—';
export function useApi<T>(path:string) {
  const [data,setData]=useState<T|null>(null),[error,setError]=useState(''),[revision,setRevision]=useState(0);
  useEffect(()=>{const controller=new AbortController();backend<T>(path,'GET',undefined,controller.signal).then(value=>{setData(value);setError('');}).catch(e=>{if(!controller.signal.aborted){setData(null);setError(e instanceof Error?e.message:'Не удалось загрузить данные');}});return()=>controller.abort();},[path,revision]);
  const reload=useCallback(()=>setRevision(v=>v+1),[]);return {data,error,reload};
}
function Failure({error}:{error:string}){return <section className="panel" role="alert"><p>{error}</p><Link href="/client/login">Войти в систему</Link></section>;}

export function LiveTicketsPage(){
  const [search,setSearch]=useState(''),[filter,setFilter]=useState(''),[page,setPage]=useState(0),[query,setQuery]=useState('');
  const q=new URLSearchParams({search:query,skip:String(page*30),take:'30'});if(filter)q.set('status',filter);
  const {data,error}=useApi<Page>('tickets?'+q),statuses=useApi<Page>('catalogs/ticket-statuses');
  if(error)return <Failure error={error}/>;
  return <div><PageHeading title={`Заявки (${data?.totalCount??'…'})`} breadcrumbs={[{label:'Заявки'}]}/><section className="panel">
    <div className="toolbar"><a className="secondary-button" href={'/api/v1/reports/tickets?'+q}>Открыть в Excel</a><form className="search-box" onSubmit={e=>{e.preventDefault();setQuery(search);setPage(0);}}><input aria-label="Поиск заявок" placeholder="Номер, тема или клиент" value={search} onChange={e=>setSearch(e.target.value)}/><button className="primary-button">Найти</button></form></div>
    <nav className="tabs"><button className={!filter?'active':''} onClick={()=>{setFilter('');setPage(0);}}>Все</button>{statuses.data?.items.filter(s=>s.Name!=='Draft').map(s=><button key={text(s.Id)} className={filter===s.Name?'active':''} onClick={()=>{setFilter(text(s.Name));setPage(0);}}>{text(s.DisplayNameMany)}</button>)}</nav>
    <div className="table-scroll"><table className="data-table"><thead><tr><th>Номер</th><th>Тема</th><th>Клиент</th><th>Специалист</th><th>Статус</th><th>Тип</th><th>Создана</th></tr></thead><tbody>{data?.items.map(t=><tr key={text(t.Id)}><td><Link href={'/tickets/'+t.Id}>{text(t.Id)}</Link></td><td><Link href={'/tickets/'+t.Id}>{text(t.Subject)}</Link></td><td>{text(t.CustomerName)}</td><td>{[t.SpecialistLastName,t.SpecialistFirstName].filter(Boolean).join(' ')||'Не назначен'}</td><td>{text(t.StatusDisplayName)}</td><td>{text(t.TypeName)}</td><td>{date(t.CreationTime)}</td></tr>)}</tbody></table></div>
    {data && !data.items.length && <p>Заявки не найдены.</p>}<div className="table-footer"><button disabled={!page} onClick={()=>setPage(v=>v-1)}>Назад</button><span>Страница {page+1}</span><button disabled={!data||(page+1)*30>=data.totalCount} onClick={()=>setPage(v=>v+1)}>Далее</button></div>
  </section></div>;
}

export { LiveTicketDetails } from './live-ticket-details';

export function LiveNotifications(){
  const [read,setRead]=useState('false'),[page,setPage]=useState(0),[notice,setNotice]=useState('');const {data,error,reload}=useApi<Page>(`notifications?read=${read}&skip=${page*30}&take=30`);
  if(error)return <Failure error={error}/>;
  return <div><PageHeading title={`Уведомления (${data?.totalCount??'…'})`} breadcrumbs={[{label:'Уведомления'}]}/><section className="panel"><nav className="tabs">{[['false','Непрочитанные'],['true','Прочитанные'],['all','Все']].map(([value,title])=><button key={value} className={read===value?'active':''} onClick={()=>{setRead(value);setPage(0);}}>{title}</button>)}</nav><table className="data-table"><thead><tr><th>Заголовок</th><th>Дата</th><th>Действие</th></tr></thead><tbody>{data?.items.map(n=><tr key={text(n.Id)}><td>{text(n.Title)}</td><td>{date(n.CreationTime)}</td><td>{!n.Seen&&<button className="secondary-button" onClick={()=>{void backend(`notifications/${n.Id}/read`,'POST',{}).then(reload).catch(e=>setNotice(e.message));}}>Прочитано</button>}</td></tr>)}</tbody></table><p role="status">{notice}</p><div className="table-footer"><button disabled={!page} onClick={()=>setPage(v=>v-1)}>Назад</button><button disabled={!data||(page+1)*30>=data.totalCount} onClick={()=>setPage(v=>v+1)}>Далее</button></div></section></div>;
}
