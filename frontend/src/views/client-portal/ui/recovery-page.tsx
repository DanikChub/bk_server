"use client";
import { useState } from 'react';
import Link from 'next/link';
export function RecoveryPage({reset=false}:{reset?:boolean}){
  const [busy,setBusy]=useState(false),[message,setMessage]=useState(''),[done,setDone]=useState(false);
  async function submit(event:React.FormEvent<HTMLFormElement>){
    event.preventDefault();const form=new FormData(event.currentTarget);setBusy(true);setMessage('');
    try{
      const body=reset?{token:new URLSearchParams(window.location.hash.slice(1)).get('token')??'',newPassword:form.get('newPassword')}:{userName:form.get('userName'),tenant:String(form.get('tenant')??'').trim()||null};
      const response=await fetch('/api/auth/'+(reset?'reset-password':'forgot-password'),{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)});
      const result=await response.json();if(!response.ok)throw new Error(result.error?.message??'Не удалось выполнить запрос');
      setDone(true);setMessage(reset?'Пароль изменён. Войдите с новым паролем.':result.message);if(reset)window.history.replaceState(null,'',window.location.pathname);
    }catch(error){setMessage(error instanceof Error?error.message:'Не удалось выполнить запрос');}finally{setBusy(false);}
  }
  return <main className="portal-login"><form onSubmit={submit}><h1>{reset?'Новый пароль':'Восстановление доступа'}</h1>{!done&&(reset?<input name="newPassword" aria-label="Новый пароль" placeholder="Новый пароль" type="password" autoComplete="new-password" minLength={8} required/>:<><input name="userName" aria-label="Логин или email" placeholder="Логин или email" autoComplete="username" required/><input name="tenant" aria-label="Организация" placeholder="Организация (для клиента)"/></>)}{!done&&<button className="primary-button" disabled={busy}>{busy?'Подождите…':reset?'Изменить пароль':'Отправить письмо'}</button>}<p role="status">{message}</p><Link href="/client/login">Вернуться ко входу</Link></form></main>;
}
