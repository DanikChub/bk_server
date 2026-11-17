import 'server-only';
import { z } from 'zod';
import { columns, identifier, insert, one, rows, transaction, update, type Row } from '../db';
import { assert } from '../errors';
import { requirePermission, requireStaff, type Session } from '../auth/session';

// Explicit allowlist; never expose an arbitrary table/column supplied by a request.
const resources: Record<string,{table:string;policy:string;fields:string[];client?:boolean;tenant?:string;readOnly?:boolean}> = {
  'ticket-types':{table:'TicketTypes',policy:'Ticket',fields:[],client:true,readOnly:true},
  'ticket-statuses':{table:'TicketStatuses',policy:'Ticket',fields:[],client:true,readOnly:true},
  'ticket-sections':{table:'TicketSections',policy:'TicketSection',fields:['Name'],client:true},
  'constraint-types':{table:'ConstraintTypes',policy:'Contract',fields:[],client:true,readOnly:true},
  'contract-statuses':{table:'ContractStatuses',policy:'ContractStatus',fields:['Title','Code'],tenant:'TenantId',client:true},
  'service-packages':{table:'ServicePackages',policy:'Contract',fields:['Name','Style'],client:true},
  'answer-templates':{table:'AnswerTemplates',policy:'AnswerTemplate',fields:['Title','Description']},
  'tags':{table:'Tags',policy:'Ticket',fields:['Name']},
  'regions':{table:'Regions',policy:'CustomerManagement',fields:['Name'],client:true},
};
function validate(table:string,allowed:string[],body:unknown,partial:boolean):Row {
  const shape:Record<string,z.ZodType>={};
  for(const c of columns(table).filter(c=>allowed.includes(c.name))) {
    let field:z.ZodType;
    if(c.type==='uuid') field=z.string().uuid();
    else if(c.type==='boolean') field=z.boolean();
    else if(['integer','smallint'].includes(c.type)) field=z.number().int();
    else if(c.type.startsWith('timestamp')) field=z.string().refine(v=>Number.isFinite(Date.parse(v)),'Некорректная дата');
    else { const max=c.type.match(/character varying\((\d+)\)/)?.[1]; field=z.string().max(max?Number(max):1_000_000); }
    if(!c.notNull) field=field.nullable();
    if(partial || !c.notNull || c.default!==null) field=field.optional();
    shape[c.name]=field;
  }
  return z.object(shape).strict().parse(body);
}
export async function resource(s:Session,name:string,id:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  const config=resources[name]; assert(config,404,'Справочник не найден');
  const read=method==='GET';
  if(s.tenantId) assert(read && config.client,403,'Доступ запрещён');
  else {requireStaff(s);requirePermission(s,config.policy+'.'+(read?'Get':method==='POST'?'Create':method==='DELETE'?'Delete':'Edit'));}
  if(!read) assert(!config.readOnly,405,'Изменение справочника недоступно');
  const table=identifier(config.table), fields=columns(config.table);
  const effectiveTenant=s.tenantId || query.get('tenantId') || null;
  if(effectiveTenant)z.string().uuid().parse(effectiveTenant);
  const predicates:string[]=[]; const values:unknown[]=[];
  if(fields.some(c=>c.name==='IsDeleted')) predicates.push('NOT "IsDeleted"');
  if(config.tenant) { values.push(effectiveTenant); predicates.push(`"${config.tenant}" IS NOT DISTINCT FROM $${values.length}::uuid`); }
  if(id) {z.string().uuid().parse(id);values.push(id);predicates.push(`"Id"=$${values.length}`);}
  const where=predicates.length?' WHERE '+predicates.join(' AND '):'';
  if(read) {
    if(id) {const row=await one(`SELECT * FROM ${table}${where}`,values);assert(row,404,'Запись не найдена');return row;}
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??100);
    const count=await one(`SELECT count(*)::integer AS count FROM ${table}${where}`,values);
    const items=await rows(`SELECT * FROM ${table}${where} ORDER BY "Id" LIMIT $${values.length+1} OFFSET $${values.length+2}`,[...values,take,skip]);
    return {items,totalCount:count!.count};
  }
  return transaction(async db=> {
    if(method==='POST') {assert(!id,405,'Некорректный маршрут');const input=validate(config.table,config.fields,body,false);if(config.tenant) input[config.tenant]=effectiveTenant;return insert(config.table,input,db,s.userId);}
    assert(id,405,'Укажите запись');
    assert(await one(`SELECT "Id" FROM ${table}${where} FOR UPDATE`,values,db),404,'Запись не найдена');
    if(method==='DELETE') { if(fields.some(c=>c.name==='IsDeleted')) return update(config.table,id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);await db.query(`DELETE FROM ${table}${where}`,values);return {ok:true}; }
    return update(config.table,id,validate(config.table,config.fields,body,true),db,s.userId);
  });
}
export { validate as validateFields };
