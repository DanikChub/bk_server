"use client";
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import { useApi } from './live-staff';
import { DesignIcon } from '@/shared/ui/design-icon/design-icon';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';
type Identity={userName:string;tenantId:string|null;roles:string[];permissions:string[]};
const links=[['Главная','/',''],['Заявки','/tickets','Ticket.Get'],['Уведомления','/notifications',''],['Клиенты','/customers','CustomerManagement.Get'],['Договоры','/contracts','Contract.Get'],['Пользователи','/users','UserManagement.Get'],['Сотрудники','/employees','EmployeeProfileManagement.Get'],['Шаблоны ответов','/answer-templates','AnswerTemplate.Get'],['Разделы заявок','/ticket-sections','TicketSection.Get'],['Пакеты услуг','/service-packages','Contract.Get']] as const;
export function LiveHeader({collapsed,onToggleMenu}:{collapsed:boolean;onToggleMenu:()=>void}){
  const me=useApi<Identity>('me'),count=useApi<{totalCount:number}>('notifications/count'),router=useRouter();
  return <header className="topbar"><button className="sidebar-toggle" onClick={onToggleMenu} aria-label={collapsed?'Открыть меню':'Свернуть меню'}><DesignIcon name="collapse"/></button><div className="topbar-actions"><span className="topbar-greeting">{me.data?`Здравствуйте, ${me.data.userName}`:''}</span><Link href="/notifications" className="notification" aria-label="Уведомления"><DesignIcon name="bell"/>{Boolean(count.data?.totalCount)&&<span>{count.data!.totalCount}</span>}</Link>{me.data?<button className="logout-button" onClick={async()=>{await fetch('/api/auth/logout',{method:'POST'});router.replace('/client/login');router.refresh();}}><DesignIcon name="logout"/>Выйти</button>:<Link href="/client/login">Войти</Link>}</div></header>;
}
export function LiveSidebar({collapsed,hidden,onNavigate}:{collapsed:boolean;hidden:boolean;onNavigate:()=>void}){
  const me=useApi<Identity>('me'),pathname=usePathname(),admin=me.data?.roles.includes('admin')&&!me.data?.tenantId;
  return <aside className="sidebar" aria-label="Основная навигация" id="app-sidebar" inert={hidden}><div className="sidebar-profile"><strong>{me.data?.userName??'Личный кабинет'}</strong></div><nav className="sidebar-nav">{links.filter(([,path,permission])=>!permission||admin||me.data?.permissions.includes(permission)||path==='/tickets').map(([title,path])=><Link href={path} key={path} onClick={onNavigate} title={collapsed?title:undefined} aria-current={pathname===path?'page':undefined} className={`sidebar-link ${pathname===path?'sidebar-link--active':''}`}><span>{title}</span></Link>)}<a className="sidebar-link" href="https://bk.kv34.ru/" target="_blank" rel="noreferrer">Библиотека консультаций</a></nav></aside>;
}
export function LiveHome(){
  const tickets=useApi<{items:Record<string,unknown>[];totalCount:number}>('tickets?take=5');
  return <div><PageHeading title="Главная"/><section className="hero-card"><span className="eyebrow">Библиотека консультаций</span><h2>Работа с клиентами и обращениями</h2><Link href="/tickets" className="primary-button">Перейти к заявкам</Link></section><section className="panel"><h2>Последние заявки {tickets.data?`(${tickets.data.totalCount})`:''}</h2>{tickets.error?<p role="alert">{tickets.error} <Link href="/client/login">Войти</Link></p>:<ul>{tickets.data?.items.map(t=><li key={String(t.Id)}><Link href={'/tickets/'+t.Id}>№ {String(t.Id)} — {String(t.Subject)}</Link></li>)}</ul>}</section></div>;
}
