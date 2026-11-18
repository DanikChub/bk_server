import 'server-only';
import { z } from 'zod';
import { rows, executor, type Executor } from '../db';
import type { Session } from '../auth/session';

export async function managerIds(s:Session,requested?:string|null,db:Executor=executor()):Promise<string[]|null>{
  if(!s.tenantId && s.roles.includes('Менеджер Отдела Продаж') && !s.roles.includes('admin')){
    const profiles=await rows(`SELECT "Id" FROM "CustomerUserProfiles" WHERE "IdentityUserId"=$1 UNION SELECT "Id" FROM "EmployeeProfiles" WHERE "IdentityUserId"=$1`,[s.userId],db);
    return profiles.map(p=>String(p.Id));
  }
  return requested?[z.string().uuid().parse(requested)]:null;
}
export function managedTenantSql(tenantExpression:string,parameter:string){
  return `EXISTS(SELECT 1 FROM "TenantProfiles" managed WHERE managed."Id"=${tenantExpression} AND NOT managed."IsDeleted" AND (managed."ResponsibleManagerId"=ANY(${parameter}::uuid[]) OR EXISTS(SELECT 1 FROM "TenantProfileManagers" link WHERE link."TenantProfileId"=managed."Id" AND link."ManagerId"=ANY(${parameter}::uuid[]))))`;
}
