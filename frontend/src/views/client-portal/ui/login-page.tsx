"use client";
/* eslint-disable @next/next/no-img-element */
import { useState } from "react";
import { PortalLink, usePortalDesign } from "@/shared/lib/client-design";
import { useRouter } from 'next/navigation';
import { usePortal } from '@/entities/client-portal/model/portal-provider';
export function ClientLoginPage() {
  const [error, setError] = useState(""); const [busy,setBusy]=useState(false);const [passwordChange,setPasswordChange]=useState(false);const router=useRouter();const {reload,clear}=usePortal();const adapted = usePortalDesign() === "adapted";
  async function submit(event:React.FormEvent<HTMLFormElement>){
    event.preventDefault();const form=new FormData(event.currentTarget);clear();setBusy(true);setError('');
    try{
      const response=await fetch('/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({userName:form.get('username'),password:form.get('password'),tenant:String(form.get('tenant')??'').trim()||null,...(form.get('authenticatorCode')?{authenticatorCode:form.get('authenticatorCode')}:{}),...(form.get('newPassword')?{newPassword:form.get('newPassword')}:{})})});
      const result=await response.json();if(!response.ok){if(result.error?.code==='password_change_required')setPasswordChange(true);throw new Error(result.error?.message??'Не удалось войти');}
      const returnTo=new URLSearchParams(window.location.search).get('returnTo');
      if(returnTo?.startsWith('/')&&!returnTo.startsWith('//')&&!returnTo.includes('\\')){window.location.assign(new URL(returnTo,window.location.origin));return;}
      const session=await fetch('/api/auth/session',{cache:'no-store'}).then(r=>r.json());if(session.tenantId)await reload();router.replace(session.tenantId?'/client':'/tickets');router.refresh();
    }catch(e){setError(e instanceof Error?e.message:'Не удалось войти');}finally{setBusy(false);}
  }
  return <main className="portal-login"><form method="post" onSubmit={submit}><h1><img src={`/design/client/${adapted ? "0252a.png" : "52de5.png"}`} width={30} height={30} alt="" />ИПС Консалтинг</h1><label className="sr-only" htmlFor="client-login">Логин</label><input id="client-login" name="username" placeholder="Логин" autoComplete="username" required /><label className="sr-only" htmlFor="client-password">Пароль</label><input id="client-password" name="password" type="password" placeholder="Пароль" autoComplete="current-password" required /><label className="sr-only" htmlFor="client-tenant">Организация</label><input id="client-tenant" name="tenant" placeholder="Организация (для клиента)" /><input name="authenticatorCode" aria-label="Код аутентификатора" placeholder="Код подтверждения (если включён)" inputMode="numeric" pattern="[0-9]{6}" maxLength={6} autoComplete="one-time-code"/>{passwordChange && <input name="newPassword" aria-label="Новый пароль" placeholder="Новый пароль" type="password" autoComplete="new-password" minLength={8} required/>}<button className="primary-button" disabled={busy}>{busy?'Вход…':'Войти'}</button><PortalLink href="/client/forgot-password">Забыли пароль?</PortalLink><p>Вход в систему доступен только авторизованным пользователям. Для регистрации свяжитесь с нами по телефону <a href="tel:88005505690">8 800 550 5690</a></p>{error && <p role="alert" className="portal-error">{error}</p>}{process.env.NEXT_PUBLIC_DATA_MODE==='demo' && <PortalLink className="portal-login-demo" href="/client">Открыть демонстрационный кабинет</PortalLink>}</form></main>;
}
