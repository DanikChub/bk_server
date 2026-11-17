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
import type { Row } from '../db';
import { customerStatistics } from './statistics';

export async function spreadsheet(s:Session,kind:string,query:URLSearchParams) {
  const loaders:Record<string,(q:URLSearchParams)=>Promise<unknown>>={tickets:q=>listTickets(s,q),contracts:q=>contracts(s,undefined,undefined,'GET',undefined,q),customers:q=>customers(s,undefined,undefined,'GET',undefined,q),users:q=>users(s,undefined,undefined,'GET',undefined,q),employees:q=>users(s,undefined,undefined,'GET',undefined,q,true),'customer-users':async q=>{const items=await customers(s,q.get('customerId')??'', 'users','GET',undefined,q);return {items,totalCount:(items as Row[]).length};},statistics:async q=>{const months=await customerStatistics(s,q.get('customerId')??'');const items=months.flatMap(m=>m.ConstraintsByMonths.map(c=>({Год:m.Year,Месяц:m.Month,Ограничение:c.Name,Лимит:c.MaxCount,Использовано:c.Sum})));return {items,totalCount:items.length};}};
  assert(loaders[kind],404,'Отчёт не найден');
  const book=new ExcelJS.Workbook(),sheet=book.addWorksheet('Отчёт');
  const fields:Record<string,[string,string][]>= {
    tickets:[['Id','Номер заявки'],['Subject','Тема'],['CustomerName','Клиент'],['TypeName','Тип'],['SectionName','Раздел'],['SpecialistLastName','Специалист'],['StatusDisplayName','Статус'],['CreationTime','Дата создания']],
    contracts:[['Name','Договор'],['CustomerName','Клиент'],['ContractStartDate','Начало'],['ContractFinishDate','Окончание'],['IsDraft','Черновик'],['IsArchive','Архив']],
    customers:[['ShortName','Клиент'],['INNNumber','ИНН'],['KPPNumber','КПП'],['Address','Адрес'],['Email','Email'],['PhoneNumber','Телефон'],['IsActive','Активен']],
    users:[['Surname','Фамилия'],['Name','Имя'],['UserName','Логин'],['Email','Email'],['PhoneNumber','Телефон'],['IsActive','Активен']],
    employees:[['Surname','Фамилия'],['Name','Имя'],['UserName','Логин'],['Email','Email'],['PhoneNumber','Телефон'],['IsActive','Активен']],
    'customer-users':[['LastName','Фамилия'],['FirstName','Имя'],['MiddleName','Отчество'],['JobPost','Должность'],['Email','Email'],['PhoneNumber','Телефон']],
    statistics:[['Год','Год'],['Месяц','Месяц'],['Ограничение','Ограничение'],['Лимит','Лимит'],['Использовано','Использовано']],
  };
  const selected=fields[kind];
  sheet.addRow(selected.map(([,title])=>title));sheet.getRow(1).font={bold:true};sheet.views=[{state:'frozen',ySplit:1}];
  for(let skip=0;;skip+=200) {
    const q=new URLSearchParams(query);q.set('skip',String(skip));q.set('take','200');
    const page=await loaders[kind](q) as {items:Row[];totalCount:number};
    for(const row of page.items) {
      // Strings remain string cells, never executable Excel formulas.
      sheet.addRow(selected.map(([key])=>typeof row[key]==='boolean'?row[key]?'Да':'Нет':row[key] instanceof Date?row[key]:row[key]??''));
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
  doc.fontSize(16).text(`Заявка № ${id}`).moveDown().fontSize(12).text(plainText(ticket.Subject)).moveDown().text(plainText(ticket.Description));
  for(const entry of ticket.history)doc.moveDown().fontSize(9).text(String(entry.CreationTime)).fontSize(11).text(plainText(entry.Description));
  doc.end();return response;
}
