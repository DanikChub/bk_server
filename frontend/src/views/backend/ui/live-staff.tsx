"use client";
import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { backend, command } from '@/entities/client-portal/api/http-repository';
import '@/widgets/tickets-list/ui/tickets-list.css';
import '@/widgets/ticket-details/ui/ticket-details.css';
import '@/widgets/notifications/ui/notifications.css';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';
import { MessageText } from '@/widgets/ticket-details/ui/message-text';

type Row=Record<string,unknown>;
type Page={items:Row[];totalCount:number};
type Identity={userId:string;tenantId:string|null;userName:string;roles:string[];permissions:string[]};
const text=(value:unknown)=>String(value??'');
const date=(value:unknown)=>value?new Date(text(value)).toLocaleString('ru-RU',{timeZone:'Europe/Moscow',dateStyle:'short',timeStyle:'short'}):'—';
const name=(r:Row|undefined)=>r?[r.LastName??r.Surname,r.FirstName??r.Name,r.MiddleName].filter(Boolean).join(' '):'—';
export function useApi<T>(path:string) {
  const [data,setData]=useState<T|null>(null),[error,setError]=useState(''),[revision,setRevision]=useState(0);
  useEffect(()=>{const controller=new AbortController();backend<T>(path,'GET',undefined,controller.signal).then(value=>{setData(value);setError('');}).catch(e=>{if(!controller.signal.aborted){setData(null);setError(e instanceof Error?e.message:'Не удалось загрузить данные');}});return()=>controller.abort();},[path,revision]);
  const reload=useCallback(()=>setRevision(v=>v+1),[]);return {data,error,reload};
}
function Failure({error}:{error:string}){return <section className="panel" role="alert"><p>{error}</p><Link href="/client/login">Войти в систему</Link></section>;}
function can(me:Identity|null,permission:string){return Boolean(me && (me.roles.includes('admin')&&!me.tenantId || me.permissions.includes(permission)));}

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

type Details=Row&{history:Row[];attachments:Row[];historyAttachments:Row[];creator:Row;customer:Row;specialist:Row;status:Row;type:Row;section:Row;limits:Row[]};
export function LiveTicketDetails({id}:{id:string}){
  const {data:t,error,reload}=useApi<Details>('tickets/'+id),me=useApi<Identity>('me');
  const statuses=useApi<Page>('catalogs/ticket-statuses'),types=useApi<Page>('catalogs/ticket-types'),sections=useApi<Page>('catalogs/ticket-sections'),specialists=useApi<Row[]>('specialists'),templates=useApi<Page>('catalogs/answer-templates');
  const [notice,setNotice]=useState(''),[busy,setBusy]=useState(false),[internal,setInternal]=useState(false),[message,setMessage]=useState(''),[files,setFiles]=useState<File[]>([]),[statusId,setStatusId]=useState(''),[constraintId,setConstraintId]=useState(''),[spent,setSpent]=useState(0),[eventTitle,setEventTitle]=useState(''),[eventDate,setEventDate]=useState('');
  async function action(path:string,body?:unknown,method='POST') {setBusy(true);try{await backend(path,method,body);setNotice('Изменения сохранены');reload();}catch(e){setNotice(e instanceof Error?e.message:'Не удалось сохранить');}finally{setBusy(false);}}
  async function reply(event:React.FormEvent){
    event.preventDefault();setBusy(true);
    try{
      await command(`tickets/${id}/reply`,{description:message,internal,...(statusId&&!internal?{statusId}:{}),...(constraintId&&!internal?{constraintTypeId:constraintId,spent}:{})},files);
      setMessage('');setFiles([]);setSpent(0);setNotice('Ответ сохранён');reload();
    }catch(e){setNotice(e instanceof Error?e.message:'Ошибка отправки');}finally{setBusy(false);}
  }
  if(error)return <Failure error={error}/>;if(!t)return <section className="panel" role="status">Загрузка заявки…</section>;
  const attachmentLink=(f:Row)=><a key={text(f.Id)} href={`/api/v1/tickets/${id}/files/${f.Id}`} download>{text(f.FileName)} ({Math.ceil(Number(f.SizeInBytes)/1024)} КБ)</a>;
  return <div><PageHeading title={`Заявка № ${id}`} breadcrumbs={[{label:'Заявки',href:'/tickets'},{label:id}]}/><div className="ticket-details-layout"><div className="ticket-details-main">
    <section className="panel"><h2>{text(t.Subject)}</h2><div className="details-grid"><div>Клиент: <strong>{text(t.customer?.LongName)}</strong></div><div>Статус: <strong>{text(t.status?.DisplayName)}</strong></div><div>Автор: {name(t.creator)}</div><div>Специалист: {name(t.specialist)}</div><div>Телефон: {text(t.creator?.PhoneNumber)}</div><div>Email: {text(t.creator?.Email)}</div><div>Создана: {date(t.CreationTime)}</div><div>Срок исполнения: {date(t.DueDate)}</div></div><MessageText text={text(t.Description)}/><div className="portal-attachments">{t.attachments.map(attachmentLink)}</div><a href={`/api/v1/tickets/${id}/files/zip`}>Скачать все файлы ZIP</a> · <a href={`/api/v1/tickets/${id}/pdf`}>Скачать PDF</a></section>
    <section className="panel conversation-panel">{t.history.map(h=><article key={text(h.Id)} className={`ticket-message ${h.IsInternal?'ticket-message--internal':''}`}><header><strong>{[h.CreatorSurname,h.CreatorName].filter(Boolean).join(' ')||'Система'}</strong> · <time>{date(h.CreationTime)}</time>{Boolean(h.IsInternal)&&<span> · Внутренняя запись</span>}</header><MessageText text={text(h.Description)}/><div className="portal-attachments">{t.historyAttachments.filter(f=>f.TicketHistoryId===h.Id).map(attachmentLink)}</div>{can(me.data,'TicketHistory.Edit')&&<button disabled={busy} className="secondary-button" onClick={()=>{const description=window.prompt('Текст сообщения',text(h.Description));if(description!==null)void action(`tickets/${id}/history/${h.Id}`,{description},'PATCH');}}>Изменить</button>}{can(me.data,'TicketHistory.Delete')&&<button disabled={busy} className="secondary-button" onClick={()=>void action(`tickets/${id}/history/${h.Id}`,undefined,'DELETE')}>Удалить запись</button>}</article>)}
    {can(me.data,'TicketHistory.Create')&&<form className="message-composer" onSubmit={reply}><label><input type="checkbox" checked={internal} disabled={busy} onChange={e=>setInternal(e.target.checked)}/>Внутреннее сообщение</label><textarea aria-label="Текст ответа" value={message} disabled={busy} onChange={e=>setMessage(e.target.value)} required rows={4}/><div className="composer-options">{!internal&&<><label>Статус<select value={statusId||text(t.TicketStatusId)} onChange={e=>setStatusId(e.target.value)}>{statuses.data?.items.map(s=><option key={text(s.Id)} value={text(s.Id)}>{text(s.DisplayName)}</option>)}</select></label><label>Ограничение<select value={constraintId} onChange={e=>setConstraintId(e.target.value)}><option value="">Без списания</option>{t.limits.map(l=><option key={text(l.Id)} value={text(l.Id)}>{text(l.Name)} ({text(l.used)}/{text(l.max)})</option>)}</select></label><label>Количество<input aria-label="Количество списаний" type="number" min={0} step={1} value={spent} onChange={e=>setSpent(Number(e.target.value))}/></label><label>Шаблон<select value="" onChange={e=>setMessage(text(templates.data?.items.find(v=>v.Id===e.target.value)?.Description))}><option value="">Выберите шаблон</option>{templates.data?.items.map(v=><option key={text(v.Id)} value={text(v.Id)}>{text(v.Title)}</option>)}</select></label></>}</div><input aria-label="Вложения" type="file" multiple onChange={e=>setFiles(Array.from(e.target.files??[]))}/><button className="primary-button" disabled={busy}>{busy?'Сохранение…':'Отправить'}</button></form>}</section>
  </div><aside className="panel ticket-actions"><h2>Действия</h2>{can(me.data,'SpecialistManagement.Create')&&<label>Специалист<select disabled={busy} value={text(t.ResponsibleId)} onChange={e=>void action(`tickets/${id}/assign`,{specialistId:e.target.value||null})}><option value="">Не назначен</option>{specialists.data?.map(v=><option key={text(v.Id)} value={text(v.Id)}>{name(v)}</option>)}</select></label>}{can(me.data,'Ticket.Edit')&&<><label>Тип<select disabled={busy} value={text(t.TicketTypeId)} onChange={e=>void action(`tickets/${id}/type`,{typeId:e.target.value})}>{types.data?.items.map(v=><option key={text(v.Id)} value={text(v.Id)}>{text(v.Name)}</option>)}</select></label><label>Раздел<select disabled={busy} value={text(t.TicketSectionId)} onChange={e=>void action(`tickets/${id}/section`,{sectionId:e.target.value||null})}><option value="">Не выбран</option>{sections.data?.items.map(v=><option key={text(v.Id)} value={text(v.Id)}>{text(v.Name)}</option>)}</select></label><button className="secondary-button" disabled={busy} onClick={()=>void action(`tickets/${id}/not-pick-up`,{})}>Не дозвонились</button></>}
    <button className="secondary-button" disabled={busy} onClick={()=>void action(`tickets/${id}/favorite`,{favorite:true})}>Добавить в избранное</button><button className="secondary-button" disabled={busy} onClick={()=>void action(`tickets/${id}/favorite`,{favorite:false})}>Убрать из избранного</button>
    <form onSubmit={e=>{e.preventDefault();void action('events',{title:eventTitle,date:new Date(eventDate).toISOString(),ticketId:id});}}><h3>Создать событие</h3><input aria-label="Текст события" required value={eventTitle} onChange={e=>setEventTitle(e.target.value)}/><input aria-label="Дата события" type="datetime-local" required value={eventDate} onChange={e=>setEventDate(e.target.value)}/><button className="primary-button" disabled={busy}>Создать</button></form>
    {can(me.data,'Ticket.Delete')&&<button className="danger-button" disabled={busy} onClick={()=>void action(`tickets/${id}`,undefined,'DELETE')}>Удалить заявку</button>}
  </aside></div><p role="status">{notice}</p></div>;
}

export function LiveNotifications(){
  const [read,setRead]=useState('false'),[page,setPage]=useState(0),[notice,setNotice]=useState('');const {data,error,reload}=useApi<Page>(`notifications?read=${read}&skip=${page*30}&take=30`);
  if(error)return <Failure error={error}/>;
  return <div><PageHeading title={`Уведомления (${data?.totalCount??'…'})`} breadcrumbs={[{label:'Уведомления'}]}/><section className="panel"><nav className="tabs">{[['false','Непрочитанные'],['true','Прочитанные'],['all','Все']].map(([value,title])=><button key={value} className={read===value?'active':''} onClick={()=>{setRead(value);setPage(0);}}>{title}</button>)}</nav><table className="data-table"><thead><tr><th>Заголовок</th><th>Дата</th><th>Действие</th></tr></thead><tbody>{data?.items.map(n=><tr key={text(n.Id)}><td>{text(n.Title)}</td><td>{date(n.CreationTime)}</td><td>{!n.Seen&&<button className="secondary-button" onClick={()=>{void backend(`notifications/${n.Id}/read`,'POST',{}).then(reload).catch(e=>setNotice(e.message));}}>Прочитано</button>}</td></tr>)}</tbody></table><p role="status">{notice}</p><div className="table-footer"><button disabled={!page} onClick={()=>setPage(v=>v-1)}>Назад</button><button disabled={!data||(page+1)*30>=data.totalCount} onClick={()=>setPage(v=>v+1)}>Далее</button></div></section></div>;
}
