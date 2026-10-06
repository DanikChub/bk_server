import type { Ticket, TicketStatus } from "@/entities/ticket/model/types";
export type Attachment = { id: string; name: string; size: number; sampleText?: string };
export type MessageKind = "client" | "reply" | "internal" | "event";
export type TicketMessage = { id: string; kind: MessageKind; author: string; text: string; createdAt: string; attachments: Attachment[] };
export type Constraint = { name: string; used: number; limit: number };
export type TicketDetail = { ticket: Ticket; updatedAt: string; messages: TicketMessage[]; constraints: Constraint[]; events: { id: string; text: string; date: string }[]; deleted: boolean };
export const ticketStatuses: TicketStatus[] = ["Новая", "В работе", "Возобновлённая", "Закрыта", "Требует уточнения"];
export const answerTemplates = [
  { name: "Добрый день!", text: "Добрый день! Уточните, пожалуйста, какие изменения вы планируете внести." },
  { name: "Ответ подготовлен", text: "Добрый день! Направляем подготовленный ответ на ваш вопрос. Если понадобятся уточнения, напишите в этой заявке." },
  { name: "Нужны документы", text: "Добрый день! Для подготовки ответа пришлите, пожалуйста, документы по вашему вопросу." },
];
export function ticketDateIso(date: string): string {
  const [day,month,year]=date.split(" ")[0].split(".");
  return `${year}-${month}-${day}T${date.split(" ")[1] || "12:00"}:00Z`;
}
export function createTicketDetail(ticket: Ticket): TicketDetail {
  const createdAt=ticketDateIso(ticket.createdAt);
  return { ticket, updatedAt: createdAt, deleted: false,
    constraints: [{name:"НПА",used:3,limit:10},{name:"Закупки",used:1,limit:5},{name:"БУ",used:2,limit:6},{name:"Торги",used:0,limit:10},{name:"Претензия",used:0,limit:5},{name:"РИС",used:0,limit:5}],
    events: [],
    messages: [
      {id:"initial",kind:"client",author:ticket.author,text:`Добрый день!\n${ticket.subject}. Прошу помочь с подготовкой ответа.`,createdAt,attachments:[{id:"sample",name:"Описание вопроса.txt",size:120,sampleText:ticket.subject}]},
      {id:"assignment",kind:"event",author:"",text:ticket.responsible ? `Заявка назначена на ${ticket.responsible}` : "Получена новая заявка",createdAt,attachments:[]},
      {id:"internal-1",kind:"internal",author:"Макаровский Вадим",text:"@ИванМорозов, посмотрите, пожалуйста, вопрос клиента.",createdAt,attachments:[]},
      {id:"internal-2",kind:"internal",author:"Морозов Иван",text:"@ВадимМакаровский, уточню детали и подготовлю ответ.",createdAt,attachments:[]},
      {id:"reply-1",kind:"reply",author:ticket.responsible || "Морозов Иван",text:"Добрый день. Какие конкретно изменения вы хотели бы внести?",createdAt,attachments:[]},
    ],
  };
}
export function deadlineProgress(ticket: Ticket, now: number): number {
  const start=Date.parse(ticketDateIso(ticket.createdAt));
  const end=Date.parse(ticketDateIso(ticket.deadline).slice(0,10)+"T23:59:59Z");
  if(end<=start)return now>=end?100:0;
  return Math.round(Math.max(0,Math.min(100,(now-start)/(end-start)*100)));
}
export function validateAttachments(files: {size:number}[]): string | null {
  return files.reduce((total,file)=>total+file.size,0)>50*1024*1024 ? "Суммарный размер файлов не должен превышать 50 МБ." : null;
}
export function sendMessage(detail: TicketDetail, input: {text:string; internal:boolean; status:TicketStatus; constraint:string; units:number; attachments:Attachment[]}, id:string, now:string): TicketDetail {
  if(!ticketStatuses.includes(input.status))throw new Error("Неизвестный статус заявки.");
  if(detail.deleted)throw new Error("Заявка удалена.");
  if(!input.text.trim())throw new Error(input.status==="Закрыта"&&!input.internal ? "Для закрытия заявки напишите ответ клиенту." : "Напишите сообщение.");
  if(!Number.isInteger(input.units)||input.units<0)throw new Error("Количество должно быть целым неотрицательным числом.");
  const constraint=detail.constraints.find(item=>item.name===input.constraint);
  if(!input.internal&&input.units>0&&(!constraint||constraint.used+input.units>constraint.limit))throw new Error("Превышен доступный лимит ограничения.");
  if(validateAttachments(input.attachments))throw new Error(validateAttachments(input.attachments)!);
  const nextStatus=input.internal?detail.ticket.status:input.status;
  const message:TicketMessage={id,kind:input.internal?"internal":"reply",author:"Морозов Иван",text:input.text.trim(),createdAt:now,attachments:input.attachments};
  const messages=[...detail.messages,message];
  if(nextStatus!==detail.ticket.status)messages.push({id:id+"-status",kind:"event",author:"",text:`Статус заявки: ${nextStatus.toLowerCase()}`,createdAt:now,attachments:[]});
  return {...detail,ticket:{...detail.ticket,status:nextStatus},updatedAt:now,messages,constraints:detail.constraints.map(item=>({...item,used:item.used+(!input.internal&&item.name===input.constraint?input.units:0)}))};
}
export function updateTicketField(detail:TicketDetail, patch:Partial<Ticket>, text:string, id:string, now:string):TicketDetail {
  return {...detail,ticket:{...detail.ticket,...patch},updatedAt:now,messages:[...detail.messages,{id,kind:"event",author:"",text,createdAt:now,attachments:[]}]};
}
export function addTicketEvent(detail:TicketDetail,text:string,date:string,id:string,now:string):TicketDetail {
  if(!text.trim()||!date||!Number.isFinite(Date.parse(date)))throw new Error("Укажите текст и дату события.");
  return {...detail,updatedAt:now,events:[...detail.events,{id,text:text.trim(),date}],messages:[...detail.messages,{id,kind:"event",author:"",text:`Событие: ${text.trim()}`,createdAt:now,attachments:[]}]};
}

function record(value:unknown):value is Record<string,unknown>{return typeof value==="object"&&value!==null&&!Array.isArray(value);}
function validAttachment(a:unknown):boolean{return record(a)&&typeof a.id==="string"&&typeof a.name==="string"&&typeof a.size==="number"&&a.size>=0&&(a.sampleText===undefined||typeof a.sampleText==="string");}
export function isDetailMap(value:unknown):value is Record<string,TicketDetail>{
  return record(value)&&Object.entries(value).every(([id,d])=>{
    if(!record(d)||!record(d.ticket))return false;const t=d.ticket;
    return String(t.id)===id&&Number.isSafeInteger(t.id)&&["subject","customer","author","responsible","tag","type","section","createdAt","deadline"].every(key=>typeof t[key]==="string")
      &&(t.responsibleId===null||typeof t.responsibleId==="string")&&ticketStatuses.includes(t.status as TicketStatus)
      &&typeof d.updatedAt==="string"&&Number.isFinite(Date.parse(d.updatedAt))&&typeof d.deleted==="boolean"
      &&Array.isArray(d.messages)&&d.messages.every(m=>record(m)&&["id","author","text","createdAt"].every(key=>typeof m[key]==="string")&&Number.isFinite(Date.parse(m.createdAt as string))&&["client","reply","internal","event"].includes(m.kind as string)&&Array.isArray(m.attachments)&&m.attachments.every(validAttachment))
      &&Array.isArray(d.constraints)&&d.constraints.every(c=>record(c)&&typeof c.name==="string"&&typeof c.used==="number"&&Number.isFinite(c.used)&&c.used>=0&&typeof c.limit==="number"&&Number.isFinite(c.limit)&&c.limit>=c.used)
      &&Array.isArray(d.events)&&d.events.every(e=>record(e)&&typeof e.id==="string"&&typeof e.text==="string"&&typeof e.date==="string"&&Number.isFinite(Date.parse(e.date)));
  });
}
