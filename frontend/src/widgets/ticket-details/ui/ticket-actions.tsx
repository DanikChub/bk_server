import { useState } from "react";
import { Bookmark, Check, Trash2 } from "lucide-react";
import { specialists,tickets } from "@/entities/ticket/model/mock";
import type { TicketDetail } from "@/entities/ticket/model/detail";
function EditField({label,value,options,onSave,free=false}:{label:string;value:string;options?:{value:string;label:string}[];onSave:(value:string)=>void;free?:boolean}){
 const [draft,setDraft]=useState<string|null>(null);
 return <div className="ticket-action-field"><label>{label}{free?<input value={draft??value} placeholder="Написать" maxLength={80} onChange={event=>setDraft(event.target.value)}/>:<select value={draft??value} onChange={event=>setDraft(event.target.value)}>{options?.map(item=><option key={item.value} value={item.value}>{item.label}</option>)}</select>}</label><button aria-label={`Сохранить: ${label}`} title="Сохранить" onClick={()=>{onSave(draft??value);setDraft(null);}}><Check size={14}/></button></div>;
}
export function TicketActions({detail,favorite,onFavorite,onDelete,onField,onEvent}:{detail:TicketDetail;favorite:boolean;onFavorite:()=>void;onDelete:()=>void;onField:(field:"specialist"|"tag"|"type"|"section",value:string)=>void;onEvent:(text:string,date:string)=>string|null}){
 const [eventText,setEventText]=useState("");const [date,setDate]=useState("");const [error,setError]=useState("");const ticket=detail.ticket;
 const choices=(field:"type"|"section")=>[...new Set([...tickets.map(t=>t[field]),ticket[field]])].map(value=>({value,label:value}));
 return <aside className="ticket-actions" aria-label="Действия с заявкой"><div className="ticket-action-buttons"><button className="secondary-button" aria-pressed={favorite} onClick={onFavorite}><Bookmark size={13} fill={favorite?"currentColor":"none"}/>{favorite?"В избранном":"В избранное"}</button><button className="danger-button" onClick={onDelete}><Trash2 size={13}/>Удалить</button></div><div className="ticket-service-packages">Пакет услуг: <span>Стандарт</span><span>IT</span></div>
 <EditField label="Специалист:" value={ticket.responsibleId??""} options={[{value:"",label:"Отсутствует"},...specialists.map(s=>({value:s.id,label:s.name}))]} onSave={value=>onField("specialist",value)}/>
 <EditField label="Метка:" value={ticket.tag} free onSave={value=>onField("tag",value.trim())}/>
 <EditField label="Тип задачи:" value={ticket.type} options={choices("type")} onSave={value=>onField("type",value)}/>
 <EditField label="Раздел:" value={ticket.section} options={choices("section")} onSave={value=>onField("section",value)}/>
 <form className="ticket-event-form" onSubmit={event=>{event.preventDefault();const problem=onEvent(eventText,date);if(problem)setError(problem);else{setEventText("");setDate("");setError("");}}}><label>Событие:<input aria-label="Дата события" type="datetime-local" value={date} onChange={event=>setDate(event.target.value)}/></label><div><input aria-label="Текст события" value={eventText} maxLength={500} onChange={event=>setEventText(event.target.value)} placeholder="Написать"/><button aria-label="Добавить событие"><Check size={14}/></button></div>{error&&<p role="alert" className="composer-error">{error}</p>}</form>
 {detail.events.length>0&&<ul className="ticket-events">{detail.events.map(event=><li key={event.id}>{event.text}<small>{new Date(event.date).toLocaleString("ru-RU",{dateStyle:"short",timeStyle:"short"})}</small></li>)}</ul>}
 </aside>;
}
