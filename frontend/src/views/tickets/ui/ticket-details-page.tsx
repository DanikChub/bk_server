"use client";
import "@/widgets/ticket-details/ui/ticket-details.css";
import Link from "next/link";
import { useEffect,useRef,useState } from "react";
import type { Ticket } from "@/entities/ticket/model/types";
import { initialFavoriteTicketIds,specialists } from "@/entities/ticket/model/mock";
import { useDemoTicketStore } from "@/entities/ticket/model/use-demo-ticket-store";
import { addTicketEvent,createTicketDetail,sendMessage,updateTicketField,type Attachment,type TicketDetail } from "@/entities/ticket/model/detail";
import { useDemoTicketSession } from "@/shared/lib/access/demo-ticket-session";
import { canEditTicketMessage, canDeleteTicketMessage, personalTicketKey, ticketAccess } from "@/shared/lib/access/ticket-access";
import { useStoredValue } from "@/shared/lib/use-stored-value";
import { ConfirmDialog } from "@/shared/ui/confirm-dialog/confirm-dialog";
import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { TicketSummary } from "@/widgets/ticket-details/ui/ticket-summary";
import { TicketConversation } from "@/widgets/ticket-details/ui/ticket-conversation";
import { MessageComposer,type ComposeInput } from "@/widgets/ticket-details/ui/message-composer";
import { TicketActions } from "@/widgets/ticket-details/ui/ticket-actions";
import { EditMessageDialog } from "@/widgets/ticket-details/ui/edit-message-dialog";
function validFavorites(value:unknown):value is number[]{return Array.isArray(value)&&value.every(id=>Number.isSafeInteger(id)&&id>0);}

export function TicketDetailsPage({ticket}:{ticket:Ticket}){
 const {session,employee,ticketsHref}=useDemoTicketSession();
 const access=ticketAccess(session);
 const {details,saveDetail}=useDemoTicketStore();const detail=details[ticket.id]??createTicketDetail(ticket);
 const [favorites,saveFavorites]=useStoredValue(personalTicketKey(session,"favorites"),employee?[]:initialFavoriteTicketIds,validFavorites);
 const [notice,setNotice]=useState("");const [confirmation,setConfirmation]=useState<string|null>(null);const [editing,setEditing]=useState<string|null>(null);const [now]=useState(()=>Date.now());const fileUrls=useRef(new Map<string,string>());
 useEffect(()=>{const urls=fileUrls.current;return()=>{urls.forEach(url=>URL.revokeObjectURL(url));};},[]);
 function save(next:TicketDetail){const stored=saveDetail(next);setNotice(stored?"Изменения сохранены в демонстрационном режиме.":"Не удалось сохранить в браузере. Изменения доступны до обновления страницы.");}
 function send(input:ComposeInput):string|null{
  if(!access.reply)return "Недостаточно прав для отправки сообщения.";
  const id=crypto.randomUUID(),createdAt=new Date().toISOString();
  const attachments=input.files.map((file,index)=>({id:`${id}-${index}`,name:file.name,size:file.size}));
  try{const next=sendMessage(detail,{...input,attachments},id,createdAt,session);input.files.forEach((file,index)=>fileUrls.current.set(attachments[index].id,URL.createObjectURL(file)));save(next);return null;}catch(error){return error instanceof Error?error.message:"Не удалось отправить сообщение.";}
 }
 function field(field:"specialist"|"tag"|"type"|"section",value:string){
  if(!access.edit){setNotice("Недостаточно прав для редактирования заявки.");return;}
  const specialist=specialists.find(s=>s.id===value);
  const patch=field==="specialist"?{responsibleId:value||null,responsible:specialist?.name??""}:{[field]:value};
  const text=field==="specialist"?specialist?`Заявка назначена на ${specialist.name}`:"Специалист снят с заявки":`${{tag:"Метка",type:"Тип",section:"Раздел"}[field]}: ${value||"отсутствует"}`;
  save(updateTicketField(detail,patch,text,crypto.randomUUID(),new Date().toISOString()));
 }
 function download(file:Attachment){
  const existing=fileUrls.current.get(file.id);
  const url=existing??(file.sampleText!==undefined?URL.createObjectURL(new Blob([file.sampleText],{type:"text/plain;charset=utf-8"})):null);
  if(!url){setNotice("Файл был прикреплён в предыдущей сессии. В демонстрационном режиме его нужно прикрепить заново.");return;}
  const anchor=document.createElement("a");anchor.href=url;anchor.download=file.name;anchor.click();if(!existing)setTimeout(()=>URL.revokeObjectURL(url),1000);
 }
 function confirmDelete(){
  if(confirmation==="ticket"&&!access.deleteTicket){setConfirmation(null);return;}
  if(confirmation&&confirmation!=="ticket"){const message=detail.messages.find(m=>m.id===confirmation);if(!message||!canDeleteTicketMessage(session,message)){setConfirmation(null);return;}}
  if(confirmation==="ticket")save({...detail,deleted:true,updatedAt:new Date().toISOString()});
  else if(confirmation){const message=detail.messages.find(m=>m.id===confirmation);message?.attachments.forEach(file=>{const url=fileUrls.current.get(file.id);if(url){URL.revokeObjectURL(url);fileUrls.current.delete(file.id);}});save({...detail,messages:detail.messages.filter(m=>m.id!==confirmation),updatedAt:new Date().toISOString()});}
  setConfirmation(null);
 }
 const editMessage=detail.messages.find(message=>message.id===editing&&canEditTicketMessage(session,message));
 if(!access.read)return <section className="panel"><h1>Нет доступа к заявке</h1></section>;
 return <div><PageHeading title={`Заявка ${ticket.id.toLocaleString("ru-RU")}`} breadcrumbs={[{label:"Заявки",href:ticketsHref},{label:`Заявка ${ticket.id.toLocaleString("ru-RU")}`}]} />
 {detail.deleted?<section className="panel deleted-ticket"><h2>Заявка удалена</h2><p>Удаление выполнено только в демонстрационном режиме.</p>{access.deleteTicket&&<button className="secondary-button" onClick={()=>save({...detail,deleted:false})}>Восстановить заявку</button>}<Link className="primary-button" href={ticketsHref}>К списку заявок</Link></section>:<div className="ticket-details-layout"><div className="ticket-details-main"><TicketSummary detail={detail} now={now}/><section className="panel conversation-panel">{access.readHistory?<TicketConversation session={session} messages={detail.messages} onDownload={download} onDelete={setConfirmation} onEdit={setEditing}/>:<p>Нет доступа к переписке.</p>}{access.reply&&<MessageComposer status={detail.ticket.status} constraints={detail.constraints} onSend={send} onNoAnswer={()=>{if(!detail.ticket.responsibleId){setNotice("Нет исполнителя заявки. Укажите исполнителя сначала.");return;}save(updateTicketField(detail,{},"Не дозвонились клиенту",crypto.randomUUID(),new Date().toISOString()));}}/>}</section>{access.reply&&<section className="panel internal-message-panel"><h2 className="sr-only">Сообщение коллеге</h2><MessageComposer internal status={detail.ticket.status} constraints={detail.constraints} onSend={send}/></section>}</div><TicketActions session={session} detail={detail} favorite={favorites.includes(ticket.id)} onFavorite={()=>{const next=favorites.includes(ticket.id)?favorites.filter(id=>id!==ticket.id):[...favorites,ticket.id];setNotice(saveFavorites(next)?"Избранное обновлено.":"Не удалось сохранить избранное в браузере.");}} onDelete={()=>setConfirmation("ticket")} onField={field} onEvent={(text,date)=>{if(!access.createEvent)return "Недостаточно прав для создания события.";try{save(addTicketEvent(detail,text,date,crypto.randomUUID(),new Date().toISOString()));return null;}catch(error){return error instanceof Error?error.message:"Не удалось добавить событие.";}}}/></div>}
 <p className="ticket-detail-notice" role="status">{notice||"Демонстрационный режим. Изменения сохраняются в этом браузере, прикреплённые файлы доступны до обновления страницы."}</p>
 <ConfirmDialog open={confirmation!==null} title={confirmation==="ticket"?"Удалить заявку?":"Удалить сообщение?"} description={confirmation==="ticket"?"Заявка исчезнет из демонстрационного списка. Её можно восстановить на этой странице.":"Сообщение будет удалено из демонстрационной переписки."} onCancel={()=>setConfirmation(null)} onConfirm={confirmDelete}/>
 {editMessage&&<EditMessageDialog key={editMessage.id} message={editMessage} onCancel={()=>setEditing(null)} onSave={text=>{if(!canEditTicketMessage(session,editMessage))return;save({...detail,updatedAt:new Date().toISOString(),messages:detail.messages.map(message=>message.id===editing?{...message,text}:message)});setEditing(null);}}/>}
 </div>;
}
