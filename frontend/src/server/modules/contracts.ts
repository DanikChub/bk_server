import 'server-only';
import { z } from 'zod';
import { insert, one, rows, transaction, update, executor, type Executor, type Row } from '../db';
import { assert } from '../errors';
import { requirePermission, requireStaff, type Session } from '../auth/session';
import { managerIds, managedTenantSql } from './manager-scope';

const input=z.object({name:z.string().min(1).max(1000),tenantId:z.string().uuid(),startsAt:z.string().datetime({offset:true}),endsAt:z.string().datetime({offset:true}),statusId:z.string().uuid(),payer:z.string().max(2000).nullable().default(null),draft:z.boolean().default(false),archive:z.boolean().default(false)}).strict();
export async function scopeContract(s:Session,id:string,db:Executor=executor(),lock=false):Promise<Row> {
  z.string().uuid().parse(id);
  if(!s.tenantId) {requireStaff(s);requirePermission(s,'Contract','Contract.Get');}
  const row=await one(`SELECT * FROM "Contracts" WHERE "Id"=$1 AND NOT "IsDeleted" ${s.tenantId?'AND "TenantId"=$2':''} ${lock?'FOR UPDATE':''}`,s.tenantId?[id,s.tenantId]:[id],db);
  assert(row,404,'Договор не найден');return row;
}
export async function contracts(s:Session,id:string|undefined,action:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  if(!s.tenantId)requirePermission(s,'Contract');
  if(method==='GET') {
    if(id) {
      const contract=await scopeContract(s,id);
      if(action==='packages'){
        const active=query.get('active');if(active)z.enum(['true','false']).parse(active);
        const items=await rows(`SELECT p.*,link."ContractStartDate",link."ContractFinishDate" FROM "ContractServicePackages" link JOIN "ServicePackages" p ON p."Id"=link."ServicePackageId" WHERE link."ContractId"=$1 ${active?`AND (link."ContractStartDate"<=now() AND link."ContractFinishDate">=now())=${active==='true'?'true':'false'}`:''} ORDER BY p."Name",p."Id"`,[id]);return {items,totalCount:items.length};
      }
      if(action==='statistics') return {constraints:await rows(`SELECT m.*,c."Name" FROM "ContractConstraintsByMonths" m JOIN "ConstraintTypes" c ON c."Id"=m."ConstraintTypeId" WHERE m."ContractId"=$1 ORDER BY m."Year" DESC,m."Month" DESC`,[id]),types:await rows(`SELECT * FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId"=$1 ORDER BY "Year" DESC,"Month" DESC`,[id]),sections:await rows(`SELECT * FROM "ContractTicketsByTicketSectionByMonths" WHERE "ContractId"=$1 ORDER BY "Year" DESC,"Month" DESC`,[id])};
      return {...contract,packages:await rows(`SELECT p.*,s."ContractStartDate",s."ContractFinishDate" FROM "ContractServicePackages" s JOIN "ServicePackages" p ON p."Id"=s."ServicePackageId" WHERE s."ContractId"=$1`,[id]),settings:await rows(`SELECT * FROM "ContractSettings" WHERE "ContractId"=$1`,[id])};
    }
    if(!s.tenantId) {requireStaff(s);requirePermission(s,'Contract.Get');}
    const tenant=s.tenantId??query.get('tenantId');if(tenant) z.string().uuid().parse(tenant);
    const values:unknown[]=[],where=['NOT c."IsDeleted"'];
    if(tenant) {values.push(tenant);where.push('c."TenantId"=$1');}
    if(query.get('active')){
      const active=z.enum(['true','false']).parse(query.get('active'))==='true';
      const policy=z.enum(['public','expiry']).parse(query.get('activePolicy')??'public');
      const rule=policy==='expiry'?'c."ContractFinishDate">=now()':`c."ContractStartDate"<=now() AND c."ContractFinishDate">=now() AND EXISTS(SELECT 1 FROM "ContractStatuses" active_status WHERE active_status."Id"=c."StatusId" AND active_status."Code"='in progress')`;
      where.push(`(${rule})=${active?'true':'false'}`);
    }
    const param=(value:unknown)=>{values.push(value);return '$'+values.length;};
    const managers=await managerIds(s,query.get('managerId'));
    if(managers)where.push(managedTenantSql('c."TenantId"',param(managers)));
    if(query.get('search'))where.push(`c."Name" ILIKE ${param('%'+query.get('search')!.trim()+'%')}`);
    if(query.get('customer')){const v=param('%'+query.get('customer')!.trim()+'%');where.push(`(tp."ShortName" ILIKE ${v} OR tp."DisplayName" ILIKE ${v})`);}
    if(query.get('statusId'))where.push(`c."StatusId"=${param(z.string().uuid().parse(query.get('statusId')))}`);
    if(query.get('packageId'))where.push(`EXISTS(SELECT 1 FROM "ContractServicePackages" p WHERE p."ContractId"=c."Id" AND p."ServicePackageId"=${param(z.string().uuid().parse(query.get('packageId')))})`);
    for(const [key,column,operator] of [['startsAfter','ContractStartDate','>='],['endsBefore','ContractFinishDate','<=']] as const){const value=query.get(key);if(value)where.push(`c."${column}"${operator}${param(z.coerce.date().parse(value))}`);}
    const skip=z.coerce.number().int().min(0).parse(query.get('skip')??0),take=z.coerce.number().int().min(1).max(200).parse(query.get('take')??100);
    const from=`FROM "Contracts" c LEFT JOIN "TenantProfiles" tp ON tp."Id"=c."TenantId" WHERE ${where.join(' AND ')}`;
    const sorts:Record<string,string>={name:'c."Name"',start:'c."ContractStartDate"',end:'c."ContractFinishDate"',customer:'tp."ShortName"'};const sort=sorts[query.get('sort')??'end'];assert(sort,400,'Неизвестная сортировка');const direction=query.get('direction')==='asc'?'ASC':'DESC';
    return {items:await rows(`SELECT c.*,tp."ShortName" AS "CustomerName" ${from} ORDER BY ${sort} ${direction},c."Id" LIMIT $${values.length+1} OFFSET $${values.length+2}`,[...values,take,skip]),totalCount:(await one(`SELECT count(*)::integer AS count ${from}`,values))!.count};
  }
  requireStaff(s);requirePermission(s,'Contract.'+(method==='POST'&&!id?'Create':method==='DELETE'?'Delete':'Edit'));
  return transaction(async db=> {
    if(!id) {
      const data=input.parse(body);assert(Date.parse(data.endsAt)>=Date.parse(data.startsAt),400,'Проверьте срок договора');
      assert(await one(`SELECT "Id" FROM "TenantProfiles" WHERE "Id"=$1 AND NOT "IsDeleted"`,[data.tenantId],db),400,'Организация не найдена');
      assert(await one(`SELECT "Id" FROM "ContractStatuses" WHERE "Id"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[data.statusId,data.tenantId],db),400,'Статус договора не найден');
      return insert('Contracts',{Name:data.name,TenantId:data.tenantId,ContractStartDate:data.startsAt,ContractFinishDate:data.endsAt,StatusId:data.statusId,ContractPayer:data.payer,IsDraft:data.draft,IsArchive:data.archive},db,s.userId);
    }
    const contract=await scopeContract(s,id,db,true);
    if(action==='packages') {
      if(method==='DELETE') {
        const data=z.object({servicePackageId:z.string().uuid()}).strict().parse(body);
        await db.query(`DELETE FROM "ContractServicePackages" WHERE "ContractId"=$1 AND "ServicePackageId"=$2`,[id,data.servicePackageId]);return {ok:true};
      }
      const data=z.object({servicePackageId:z.string().uuid(),startsAt:z.string().datetime({offset:true}),endsAt:z.string().datetime({offset:true})}).strict().parse(body);
      assert(Date.parse(data.endsAt)>=Date.parse(data.startsAt),400,'Проверьте срок пакета');
      if(method==='DELETE') await db.query(`DELETE FROM "ContractServicePackages" WHERE "ContractId"=$1 AND "ServicePackageId"=$2`,[id,data.servicePackageId]);
      else await db.query(`INSERT INTO "ContractServicePackages"("ContractId","ServicePackageId","ContractStartDate","ContractFinishDate") VALUES($1,$2,$3,$4) ON CONFLICT("ContractId","ServicePackageId") DO UPDATE SET "ContractStartDate"=excluded."ContractStartDate","ContractFinishDate"=excluded."ContractFinishDate"`,[id,data.servicePackageId,data.startsAt,data.endsAt]);
      return {ok:true};
    }
    if(action==='replace-package'){
      const data=z.object({servicePackageId:z.string().uuid()}).strict().parse(body);
      assert(await one(`SELECT "Id" FROM "ServicePackages" WHERE "Id"=$1`,[data.servicePackageId],db),400,'Пакет не найден');
      await db.query(`DELETE FROM "ContractServicePackages" WHERE "ContractId"=$1`,[id]);
      await db.query(`INSERT INTO "ContractServicePackages"("ContractId","ServicePackageId","ContractStartDate","ContractFinishDate") VALUES($1,$2,$3,$4)`,[id,data.servicePackageId,contract.ContractStartDate,contract.ContractFinishDate]);return {ok:true};
    }
    if(action==='limits') {
      const data=z.object({constraintTypeId:z.string().uuid(),max:z.number().int().min(0).max(1_000_000)}).strict().parse(body);
      const existing=await one(`SELECT "Id" FROM "ContractSettings" WHERE "ContractId"=$1 AND "ConstraintTypeId"=$2 ORDER BY "CreationTime" DESC LIMIT 1 FOR UPDATE`,[id,data.constraintTypeId],db);
      return existing?update('ContractSettings',existing.Id,{Max:data.max},db,s.userId):insert('ContractSettings',{ContractId:id,ConstraintTypeId:data.constraintTypeId,Max:data.max},db,s.userId);
    }
    if(method==='DELETE') return update('Contracts',id,{IsDeleted:true,DeletionTime:new Date(),DeleterId:s.userId},db,s.userId);
    const data=input.omit({tenantId:true}).partial().parse(body);
    assert(Date.parse(data.endsAt??String(contract.ContractFinishDate))>=Date.parse(data.startsAt??String(contract.ContractStartDate)),400,'Проверьте срок договора');
    if(data.statusId) assert(await one(`SELECT "Id" FROM "ContractStatuses" WHERE "Id"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[data.statusId,contract.TenantId],db),400,'Статус договора не найден');
    const mapping:Record<string,string>={name:'Name',startsAt:'ContractStartDate',endsAt:'ContractFinishDate',statusId:'StatusId',payer:'ContractPayer',draft:'IsDraft',archive:'IsArchive'};
    return update('Contracts',id,Object.fromEntries(Object.entries(data).map(([k,v])=>[mapping[k],v])),db,s.userId);
  });
}
