"use client";
import { useState } from 'react';
import { useApi } from './live-staff';
import { backend } from '@/entities/client-portal/api/http-repository';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';
type Row=Record<string,unknown>;
const catalogs={
  'answer-templates':{title:'Шаблоны ответов',fields:[['Title','Название'],['Description','Текст']]},
  'ticket-sections':{title:'Разделы заявок',fields:[['Name','Название']]},
  'service-packages':{title:'Пакеты услуг',fields:[['Name','Название'],['Style','Стиль']]},
  tags:{title:'Теги',fields:[['Name','Название']]},
} as const;
export function LiveCatalog({catalog}:{catalog:keyof typeof catalogs}){
  const config=catalogs[catalog],path='catalogs/'+catalog,{data,error,reload}=useApi<{items:Row[]}>(path);const [editing,setEditing]=useState<Row|null>(null),[notice,setNotice]=useState('');
  async function save(e:React.FormEvent<HTMLFormElement>){e.preventDefault();const form=new FormData(e.currentTarget);try{await backend(path+(editing?.Id?'/'+editing.Id:''),editing?.Id?'PATCH':'POST',Object.fromEntries(config.fields.map(([field])=>[field,form.get(field)])));setEditing(null);setNotice('Сохранено');reload();}catch(error){setNotice((error as Error).message);}}
  return <div><PageHeading title={config.title} breadcrumbs={[{label:'Администрирование'},{label:config.title}]}/><section className="panel">{error&&<p role="alert">{error}</p>}<button className="primary-button" onClick={()=>setEditing({})}>Добавить</button>{editing&&<form key={String(editing.Id??'new')} onSubmit={save}>{config.fields.map(([field,label])=><label key={field}>{label}{field==='Description'?<textarea name={field} required defaultValue={String(editing[field]??'')} rows={6}/>:<input name={field} required={field!=='Style'} defaultValue={String(editing[field]??'')}/>}</label>)}<button className="primary-button">Сохранить</button><button type="button" className="secondary-button" onClick={()=>setEditing(null)}>Отмена</button></form>}<table className="data-table"><thead><tr>{config.fields.map(([field,title])=><th key={field}>{title}</th>)}<th>Действия</th></tr></thead><tbody>{data?.items.map(row=><tr key={String(row.Id)}>{config.fields.map(([field])=><td key={field}>{String(row[field]??'')}</td>)}<td><button className="secondary-button" onClick={()=>setEditing(row)}>Изменить</button><button className="secondary-button" onClick={()=>{void backend(path+'/'+row.Id,'DELETE').then(reload).catch(e=>setNotice(e.message));}}>Удалить</button></td></tr>)}</tbody></table><p role="status">{notice}</p></section></div>;
}
