"use client";
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState, type ReactNode } from 'react';
import { useApi } from '@/views/backend/ui/live-staff';
import { backend } from '@/entities/client-portal/api/http-repository';
import { PageHeading } from '@/shared/ui/page-heading/page-heading';
import { NewDesignIcon } from '@/shared/ui/design-icon/new-design-icon';
import '@fontsource-variable/inter';
import './customer-editor.css';

type Row=Record<string,unknown>;
type Contact={fullName:string;jobPost:string;phone:string;email:string;birthday:string|null;userName:string;password:string};
type Editor={company:{LongName:string;ShortName:string;INNNumber:string;KPPNumber:string;PhoneNumber:string;Email:string;SiteUrl:string;RegionId:string|null;Address:string;CommentNotes:string};contact:Contact|null;responsibleManagerId:string|null;meta:{organizationType:string;networkEdition:string;subordinateIds:string[];bitrixUrl:string}};
const empty:Editor={company:{LongName:'',ShortName:'',INNNumber:'',KPPNumber:'',PhoneNumber:'',Email:'',SiteUrl:'',RegionId:null,Address:'',CommentNotes:''},contact:null,responsibleManagerId:null,meta:{organizationType:'administration',networkEdition:'',subordinateIds:[],bitrixUrl:''}};
const blankContact:Contact={fullName:'',jobPost:'',phone:'',email:'',birthday:null,userName:'',password:''};
const name=(row:Row)=>[row.LastName,row.FirstName,row.MiddleName].filter(Boolean).join(' ');
export function CustomerEditorPage({id}:{id?:string}){
  const {data,error}=useApi<Editor>(id?`customers/${id}/edit-form`:'me');
  if(id&&error)return <section className="panel" role="alert">{error}</section>;
  if(id&&!data)return <section className="panel" role="status">Загрузка клиента…</section>;
  return <CustomerEditor key={id??'new'} id={id} initial={id?data!:empty}/>;
}
function CustomerEditor({id,initial}:{id?:string;initial:Editor}){
  const router=useRouter(),[value,setValue]=useState<Editor>(initial),[contact,setContact]=useState<Contact>(initial.contact??blankContact),[busy,setBusy]=useState(false),[error,setError]=useState(''),[subordinate,setSubordinate]=useState('');
  const regions=useApi<{items:Row[]}>('catalogs/regions?take=200'),managers=useApi<Row[]>('specialists'),companies=useApi<{items:Row[]}>('customers?take=200');
  function company(key:keyof Editor['company'],next:string|null){setValue(v=>({...v,company:{...v.company,[key]:next}}));}
  function meta(key:keyof Editor['meta'],next:string|string[]){setValue(v=>({...v,meta:{...v.meta,[key]:next}}));}
  function contactField(key:keyof Contact,next:string|null){setContact(v=>({...v,[key]:next}));}
  async function save(event:React.FormEvent){
    event.preventDefault();setBusy(true);setError('');
    try{const hasContact=Object.values(contact).some(v=>Boolean(v));const result=await backend<{id:string}>(id?`customers/${id}/edit-form`:'customers/new',id?'PATCH':'POST',{...value,contact:hasContact?contact:null});router.push('/customers/'+result.id);router.refresh();}
    catch(e){setError(e instanceof Error?e.message:'Не удалось сохранить клиента');}finally{setBusy(false);}
  }
  const contactEnabled=Object.values(contact).some(v=>Boolean(v));
  function field(label:string,control:ReactNode,nested=false){return <div className={`customer-form-row${nested?' customer-form-row--nested':''}`}><label>{label}</label><div>{control}</div></div>;}
  const input=(key:keyof Editor['company'],placeholder:string,required=false,type='text')=><input aria-label={key==='Email'?'E-mail организации':key==='PhoneNumber'?'Телефон организации':placeholder} name={key} type={type} required={required} value={value.company[key]??''} onChange={e=>company(key,e.target.value)} placeholder={placeholder} maxLength={key==='ShortName'?1000:2000}/>;
  const contactInput=(key:keyof Contact,label:string,placeholder:string,type='text',required=false)=><input aria-label={label} type={type} required={required&&contactEnabled} value={contact[key]??''} onChange={e=>contactField(key,key==='birthday'?e.target.value||null:e.target.value)} placeholder={placeholder} autoComplete={key==='password'?'new-password':'off'} minLength={key==='password'?8:undefined} maxLength={key==='phone'?16:key==='password'?1024:256}/>;
  return <div className="customer-editor"><PageHeading title={id?'Редактирование клиента':'Новый клиент'} breadcrumbs={[{label:'Клиенты',href:'/customers'},{label:id?'Редактирование клиента':'Добавить нового клиента'}]}/>
    <form method="post" className="panel customer-editor-panel" onSubmit={save}><h2>{id?'Редактирование клиента:':'Новый клиент:'}</h2><fieldset disabled={busy}>
      {field('Наименование',input('LongName','Введите наименование организации',true))}
      {field('Краткое наименование',input('ShortName','Введите краткое наименование организации',true))}
      {field('Тип организации',<select aria-label="Тип организации" className="customer-form-select" value={value.meta.organizationType} onChange={e=>meta('organizationType',e.target.value)}>{[['administration','Администрация'],['institution','Учреждения (МУП, МКУ, МБУ)'],['education','Образовательные учреждения'],['commercial','Коммерческие организации (ООО, АО, ИП, ФЛ)']].map(([key,title])=><option key={key} value={key}>{title}</option>)}</select>)}
      {field('ИНН / КПП',<div className="customer-inn-pair">{input('INNNumber','Введите ИНН организации')}<span>/</span>{input('KPPNumber','Введите КПП организации')}</div>)}
      {field('Телефон',input('PhoneNumber','Введите телефон организации'))}
      {field('E-mail',input('Email','Введите e-mail организации',false,'email'))}
      {field('Сайт',<div className="customer-site-pair">{input('SiteUrl','Введите адрес сайта организации')}<label htmlFor="network-edition">СИ:</label><input id="network-edition" value={value.meta.networkEdition} onChange={e=>meta('networkEdition',e.target.value)} placeholder="Введите данные сетевого издания"/></div>)}
      {field('Регион',<select aria-label="Регион" className="customer-form-select" value={value.company.RegionId??''} onChange={e=>company('RegionId',e.target.value||null)}><option value="">Выберите регион</option>{regions.data?.items.map(r=><option key={String(r.Id)} value={String(r.Id)}>{String(r.Name)}</option>)}</select>)}
      {field('Адрес',input('Address','Введите адрес организации'))}
      {field('Подведы',<div><div className="customer-subordinate-picker"><select aria-label="Добавить подведомственную организацию" value={subordinate} onChange={e=>setSubordinate(e.target.value)}><option value="">Выберите подведомственную организацию</option>{companies.data?.items.filter(c=>c.Id!==id&&!value.meta.subordinateIds.includes(String(c.Id))).map(c=><option key={String(c.Id)} value={String(c.Id)}>{String(c.ShortName)}</option>)}</select><button type="button" aria-label="Добавить подведомственную организацию" disabled={!subordinate} onClick={()=>{meta('subordinateIds',[...value.meta.subordinateIds,subordinate]);setSubordinate('');}}>＋</button></div>{value.meta.subordinateIds.length>0&&<ul className="customer-selected-companies">{value.meta.subordinateIds.map(child=><li key={child}>{String(companies.data?.items.find(c=>c.Id===child)?.ShortName??child)}<button type="button" aria-label="Убрать подведомственную организацию" onClick={()=>meta('subordinateIds',value.meta.subordinateIds.filter(v=>v!==child))}>×</button></li>)}</ul>}</div>)}
      {field('Контактное лицо',contactInput('fullName','ФИО контактного лица','Введите ФИО контактного лица','text',true))}
      {field('Должность',contactInput('jobPost','Должность контактного лица','Введите должность контактного лица'),true)}
      {field('Телефон',contactInput('phone','Телефон контактного лица','Введите телефон контактного лица'),true)}
      {field('E-mail',contactInput('email','E-mail контактного лица','Введите e-mail контактного лица','email',true),true)}
      {field('Дата рождения',contactInput('birthday','Дата рождения контактного лица','Введите дату рождения контактного лица','date'),true)}
      {field('Логин',contactInput('userName','Логин контактного лица','Введите логин контактного лица','text',true),true)}
      {field('Пароль',contactInput('password','Пароль контактного лица',id?'Оставьте пустым, чтобы сохранить пароль':'Введите пароль контактного лица','password',!initial.contact),true)}
      {field('Ответственный',<select aria-label="Ответственный" value={value.responsibleManagerId??''} onChange={e=>setValue(v=>({...v,responsibleManagerId:e.target.value||null}))}><option value="">Выберите ответственного сотрудника</option>{managers.data?.map(m=><option key={String(m.Id)} value={String(m.Id)}>{name(m)}</option>)}</select>)}
      {field('Примечание',input('CommentNotes','Примечания по текущему клиенту'))}
      {id&&field('Ссылка на Битрикс',<input aria-label="Ссылка на Битрикс" type="url" value={value.meta.bitrixUrl} onChange={e=>meta('bitrixUrl',e.target.value)} placeholder="https://…"/>)}
    </fieldset>{error&&<p className="customer-form-error" role="alert">{error}</p>}<footer><Link href={id?'/customers/'+id:'/customers'} className="secondary-button"><span className="customer-back-icon"><NewDesignIcon file="8adea.svg"/></span>Отменить</Link><button className="primary-button" disabled={busy}>{busy?'Сохранение…':id?'Сохранить':'Создать'}</button></footer></form>
  </div>;
}
