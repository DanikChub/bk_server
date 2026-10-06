"use client";
import { useEffect,useId,useRef } from "react";
export function ConfirmDialog({open,title,description,onCancel,onConfirm}:{open:boolean;title:string;description:string;onCancel:()=>void;onConfirm:()=>void}){
  const ref=useRef<HTMLDialogElement>(null);const id=useId();
  useEffect(()=>{if(open&&!ref.current?.open)ref.current?.showModal();else if(!open&&ref.current?.open)ref.current.close();},[open]);
  return <dialog ref={ref} className="confirm-dialog" aria-labelledby={id} onCancel={event=>{event.preventDefault();onCancel();}}><h2 id={id}>{title}</h2><p>{description}</p><div><button className="secondary-button" onClick={onCancel}>Отмена</button><button className="danger-button" onClick={onConfirm}>Удалить</button></div></dialog>;
}
