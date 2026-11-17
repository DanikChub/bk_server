import 'server-only';
import { z } from 'zod';
import { randomUUID } from 'node:crypto';
import { one, rows, insert, transaction, update } from '../db';
import { isHostAdmin, type Session } from '../auth/session';
import { assert } from '../errors';
export async function roles(s:Session,id:string|undefined,action:string|undefined,method:string,body:unknown,query:URLSearchParams) {
  assert(isHostAdmin(s),403,'Требуется администратор');
  if(method==='GET') {
    const tenant=query.get('tenantId');if(tenant)z.string().uuid().parse(tenant);
    if(!id)return rows(`SELECT * FROM "AbpRoles" WHERE "TenantId" IS NOT DISTINCT FROM $1::uuid ORDER BY "Name"`,[tenant]);
    z.string().uuid().parse(id);const role=await one(`SELECT * FROM "AbpRoles" WHERE "Id"=$1`,[id]);assert(role,404,'Роль не найдена');
    return {...role,permissions:await rows(`SELECT "Name" FROM "AbpPermissionGrants" WHERE "ProviderName"='R' AND "ProviderKey"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[role.Name,role.TenantId])};
  }
  return transaction(async db=>{
    if(!id) {
      const data=z.object({name:z.string().trim().min(1).max(256),tenantId:z.string().uuid().nullable().default(null)}).strict().parse(body);
      await db.query(`SELECT pg_advisory_xact_lock(hashtextextended($1,0))`,[`role:${data.tenantId??'host'}:${data.name.toUpperCase()}`]);
      assert(!await one(`SELECT "Id" FROM "AbpRoles" WHERE "NormalizedName"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[data.name.toUpperCase(),data.tenantId],db),409,'Роль уже существует');
      return insert('AbpRoles',{Name:data.name,NormalizedName:data.name.toUpperCase(),TenantId:data.tenantId,EntityVersion:0,IsDefault:false,IsPublic:true,IsStatic:false},db,s.userId);
    }
    z.string().uuid().parse(id);const role=await one(`SELECT * FROM "AbpRoles" WHERE "Id"=$1 FOR UPDATE`,[id],db);assert(role,404,'Роль не найдена');
    if(action==='permissions') {
      assert(role.Name!=='admin',409,'Права системного администратора не редактируются');
      const data=z.object({permissions:z.array(z.string().min(1).max(128)).max(500)}).strict().parse(body);
      await db.query(`DELETE FROM "AbpPermissionGrants" WHERE "ProviderName"='R' AND "ProviderKey"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[role.Name,role.TenantId]);
      for(const name of new Set(data.permissions))await insert('AbpPermissionGrants',{Id:randomUUID(),TenantId:role.TenantId,Name:name,ProviderName:'R',ProviderKey:role.Name},db);
      return {ok:true};
    }
    assert(!role.IsStatic,409,'Системную роль нельзя изменить');
    if(method==='DELETE') {
      await db.query(`DELETE FROM "AbpUserRoles" WHERE "RoleId"=$1`,[id]);
      await db.query(`DELETE FROM "AbpPermissionGrants" WHERE "ProviderName"='R' AND "ProviderKey"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid`,[role.Name,role.TenantId]);
      await db.query(`DELETE FROM "AbpRoles" WHERE "Id"=$1`,[id]);return {ok:true};
    }
    const {name}=z.object({name:z.string().trim().min(1).max(256)}).strict().parse(body);
    await db.query(`SELECT pg_advisory_xact_lock(hashtextextended($1,0))`,[`role:${role.TenantId??'host'}:${name.toUpperCase()}`]);
    assert(!await one(`SELECT "Id" FROM "AbpRoles" WHERE "NormalizedName"=$1 AND "TenantId" IS NOT DISTINCT FROM $2::uuid AND "Id"<>$3`,[name.toUpperCase(),role.TenantId,id],db),409,'Роль уже существует');
    await db.query(`UPDATE "AbpPermissionGrants" SET "ProviderKey"=$1 WHERE "ProviderName"='R' AND "ProviderKey"=$2 AND "TenantId" IS NOT DISTINCT FROM $3::uuid`,[name,role.Name,role.TenantId]);
    return update('AbpRoles',id,{Name:name,NormalizedName:name.toUpperCase()},db,s.userId);
  });
}
