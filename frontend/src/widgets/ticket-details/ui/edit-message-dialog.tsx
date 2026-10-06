import { useEffect,useId,useRef,useState } from "react";
import type { TicketMessage } from "@/entities/ticket/model/detail";
export function EditMessageDialog({message,onCancel,onSave}:{message:TicketMessage;onCancel:()=>void;onSave:(text:string)=>void}){
 const ref=useRef<HTMLDialogElement>(null);const id=useId();const [text,setText]=useState(message.text);const [error,setError]=useState("");
 useEffect(()=>{if(!ref.current?.open)ref.current?.showModal();},[]);
 return <dialog ref={ref} className="confirm-dialog edit-message-dialog" aria-labelledby={id} onCancel={event=>{event.preventDefault();onCancel();}}><form onSubmit={event=>{event.preventDefault();if(!text.trim()){setError("Сообщение не должно быть пустым.");return;}onSave(text.trim());}}><h2 id={id}>Редактировать сообщение</h2><textarea aria-label="Текст сообщения" value={text} onChange={event=>setText(event.target.value)} rows={6}/>{error&&<p role="alert" className="composer-error">{error}</p>}<div className="dialog-buttons"><button className="secondary-button" type="button" onClick={onCancel}>Отмена</button><button className="primary-button">Сохранить</button></div></form></dialog>;
}
