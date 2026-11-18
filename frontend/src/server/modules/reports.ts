import 'server-only';
import { Readable } from 'node:stream';
import path from 'node:path';
import ExcelJS from 'exceljs';
import PDFDocument from 'pdfkit';
import { assert } from '../errors';
import type { Session } from '../auth/session';
import { listTickets, ticketDetails } from './tickets';
import { contracts } from './contracts';
import { customers } from './customers';
import { users } from './users';
import { one, rows, type Row } from '../db';
import { customerStatistics } from './statistics';

export async function spreadsheet(s:Session,kind:string,query:URLSearchParams) {
  const loaders:Record<string,(q:URLSearchParams)=>Promise<unknown>>={tickets:q=>listTickets(s,q),contracts:q=>contracts(s,undefined,undefined,'GET',undefined,q),customers:q=>customers(s,undefined,undefined,'GET',undefined,q),users:q=>users(s,undefined,undefined,'GET',undefined,q),employees:q=>users(s,undefined,undefined,'GET',undefined,q,true),'customer-users':async q=>{
    const items=await customers(s,q.get('customerId')??'', 'users','GET',undefined,q) as Row[];
    return {items:items.slice(Number(q.get('skip')),Number(q.get('skip'))+Number(q.get('take'))),totalCount:items.length};
  },statistics:async q=>{
    const months=await customerStatistics(s,q.get('customerId')??'');
    const items=months.flatMap(m=>Array.from({length:Math.max(m.TicketSectionsByMonths.length,m.TicketTypesByMonths.length,m.ConstraintsByMonths.length)},(_,i)=>({Section:m.TicketSectionsByMonths[i]?.Name,SectionCount:m.TicketSectionsByMonths[i]?.Count,Type:m.TicketTypesByMonths[i]?.Name,TypeCount:m.TicketTypesByMonths[i]?.Count,Constraint:m.ConstraintsByMonths[i]?.Name,Spent:m.ConstraintsByMonths[i]?.Sum,Period:new Date(Date.UTC(m.Year,m.Month-1,1)).toLocaleDateString('ru-RU',{month:'long',year:'numeric',timeZone:'UTC'})})));
    return {items:items.slice(Number(q.get('skip')),Number(q.get('skip'))+Number(q.get('take'))),totalCount:items.length};
  }};
  assert(loaders[kind],404,'Отчёт не найден');
  const book=new ExcelJS.Workbook(),sheet=book.addWorksheet('Отчёт');
  const fields:Record<string,[string,string][]>= {
    tickets:[['Id','Номер заявки'],['CustomerName','Наименование клиента'],['LastTagName','Тэг'],['TypeName','Тип'],['SectionName','Раздел'],['SpecialistLastName','Специалист'],['CreationTime','Дата создания']],
    contracts:[['CustomerName','Название клиента'],['Name','Номер договора'],['PackageNames','Пакет услуг'],['ContractStartDate','Дата начала'],['ContractFinishDate','Дата окончания']],
    customers:[['ShortName','Короткое Название'],['LongName','Полное Название'],['Address','Адрес']],
    users:[['FullName','ФИО'],['JobPost','Должность'],['Direction','Направление деятельности'],['CustomerName','Наименование клиента'],['ActualizationTime','Актуализация']],
    employees:[['Surname','Фамилия'],['Name','Имя'],['UserName','Логин'],['Email','Email'],['PhoneNumber','Телефон'],['IsActive','Активен']],
    'customer-users':[['FullName','ФИО'],['JobPost','Должность'],['CustomerName','Наименование клиента']],
    statistics:[['Section','Раздел Название'],['SectionCount','Раздел количество'],['Type','Тип Название'],['TypeCount','Тип Количество'],['Constraint','Ограничение Название'],['Spent','Ограничение Количество'],['Period','Дата']],
  };
  const selected=fields[kind];
  sheet.addRow(selected.map(([,title])=>title));sheet.getRow(1).font={bold:true};sheet.views=[{state:'frozen',ySplit:1}];
  for(let skip=0;;skip+=200) {
    const q=new URLSearchParams(query);q.set('skip',String(skip));q.set('take','200');
    const page=await loaders[kind](q) as {items:Row[];totalCount:number};
    const names=new Map<string,unknown>();
    if(kind==='tickets')for(const item of await rows(`SELECT l."TicketId" AS id,string_agg(tg."Name",', ' ORDER BY tg."Name") AS value FROM "TicketTags" l JOIN "Tags" tg ON tg."Id"=l."TagId" WHERE l."TicketId"=ANY($1::bigint[]) GROUP BY l."TicketId"`,[page.items.map(r=>r.Id)]))names.set(String(item.id),item.value);
    if(kind==='contracts')for(const item of await rows(`SELECT l."ContractId" AS id,string_agg(p."Name",', ' ORDER BY p."Name") AS value FROM "ContractServicePackages" l JOIN "ServicePackages" p ON p."Id"=l."ServicePackageId" WHERE l."ContractId"=ANY($1::uuid[]) GROUP BY l."ContractId"`,[page.items.map(r=>r.Id)]))names.set(String(item.id),item.value);
    const company=kind==='customer-users'?await one(`SELECT coalesce("DisplayName","ShortName") AS name FROM "TenantProfiles" WHERE "Id"=$1`,[query.get('customerId')]):null;
    for(const row of page.items) {
      row.FullName=[row.LastName??row.Surname,row.FirstName??row.Name,row.MiddleName].filter(Boolean).join(' ');
      if(kind==='tickets')row.LastTagName=names.get(String(row.Id))??'';
      if(kind==='contracts')row.PackageNames=names.get(String(row.Id))??'Отсутствует';
      if(kind==='customer-users')row.CustomerName=company?.name??'';
      // Strings remain string cells, never executable Excel formulas.
      const added=sheet.addRow(selected.map(([key])=>typeof row[key]==='boolean'?row[key]?'Да':'Нет':row[key] instanceof Date?row[key]:row[key]??''));
      selected.forEach(([key],index)=>{if(row[key] instanceof Date)added.getCell(index+1).numFmt='dd.mm.yyyy hh:mm';});
    }
    if(skip+page.items.length>=page.totalCount || !page.items.length)break;
    assert(skip<100_000,413,'Отчёт слишком большой для синхронной выгрузки');
  }
  sheet.columns.forEach(column=>{column.width=24;});
  const bytes=await book.xlsx.writeBuffer();
  return new Response(new Uint8Array(bytes),{headers:{'Content-Type':'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet','Content-Disposition':`attachment; filename="${kind}.xlsx"`,'Cache-Control':'private, no-store'}});
}
function plainText(value:unknown) {return String(value??'').replace(/<[^>]*>/g,' ').replace(/&nbsp;/g,' ').replace(/&amp;/g,'&');}
export async function ticketPdf(s:Session,id:string) {
  const ticket=await ticketDetails(s,id),doc=new PDFDocument({size:'A4',margin:45});
  doc.font(path.join(process.cwd(),'public/fonts/open-sans.ttf'));
  const response=new Response(Readable.toWeb(doc) as ReadableStream,{headers:{'Content-Type':'application/pdf','Content-Disposition':`attachment; filename="ticket-${id}.pdf"`,'Cache-Control':'private, no-store'}});
  const date=(value:unknown)=>value?new Date(String(value)).toLocaleString('ru-RU',{timeZone:'Europe/Moscow'}):'—';
  const status=ticket.status as Row|undefined,type=ticket.type as Row|undefined,section=ticket.section as Row|undefined,customer=ticket.customer as Row|undefined,specialist=ticket.specialist as Row|undefined;
  const packages=ticket.ContractId?await rows(`SELECT p."Name" FROM "ContractServicePackages" l JOIN "ServicePackages" p ON p."Id"=l."ServicePackageId" WHERE l."ContractId"=$1 ORDER BY p."Name"`,[ticket.ContractId]):[];
  doc.fontSize(16).text(`Заявка № ${id}`).moveDown().fontSize(10);
  for(const [label,value] of [
    ['Клиент',customer?.LongName??customer?.ShortName],['Дата создания',date(ticket.CreationTime)],['Дата изменения',date(ticket.LastModificationTime??ticket.CreationTime)],
    ['Статус',status?.DisplayName??status?.Name],['Пакеты услуг',packages.map(p=>p.Name).join(', ')||'Отсутствует'],['Раздел',section?.Name],
    ['Срок ответа',date(ticket.DueDate)],['Тип',type?.Name],['Специалист',[specialist?.LastName,specialist?.FirstName,specialist?.MiddleName,specialist?.JobPost].filter(Boolean).join(' ')],
  ])doc.text(`${label}: ${plainText(value)||'—'}`);
  doc.moveDown().fontSize(12).text(plainText(ticket.Subject)).moveDown().text(plainText(ticket.Description));
  for(const entry of ticket.history)doc.moveDown().fontSize(9).text([date(entry.CreationTime),entry.CreatorSurname,entry.CreatorName,entry.IsInternal?'Внутренняя запись':null].filter(Boolean).join(' · ')).fontSize(11).text(plainText(entry.Description));
  doc.end();return response;
}
