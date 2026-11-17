import 'server-only';
import { randomUUID } from 'node:crypto';
import { one, insert, transaction } from './db';
import { createUser } from './modules/users';

const statuses=[['Renewed','Возобновленные','Возобновленная','fa-inbox',null],['New','Новые','Новая','fa-newspaper','label-info'],['InProgress','В работе','В работе','fa fa-envelope-o','label-warning'],['WaitingForSpecialistResponse','Ожидание ответа','Ожидание ответа','fa-inbox',null],['MoreDetails','Требуется уточнение','Требуется уточнение','fa fa-certificate','label-danger'],['Answered','Закрытые','Закрыта','fa-archive','label-success'],['Draft','Черновики','Черновик','fa-pen','label-secondary']];
export async function bootstrap(admin?:{userName:string;email:string;password:string}) {
  return transaction(async db=>{
    await db.query(`SELECT pg_advisory_xact_lock(826411090)`);
    for(const [Name,DisplayNameMany,DisplayName,Icon,Style] of statuses) {
      if(!await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"=$1`,[Name],db))await insert('TicketStatuses',{Name,DisplayNameMany,DisplayName,Icon,Style,IsPublic:true},db);
    }
    if(!await one(`SELECT "Id" FROM "AbpRoles" WHERE "NormalizedName"='ADMIN' AND "TenantId" IS NULL`,[],db))await insert('AbpRoles',{Id:randomUUID(),Name:'admin',NormalizedName:'ADMIN',TenantId:null,IsDefault:false,IsPublic:false,IsStatic:true,EntityVersion:0},db);
    if(!admin)return {seeded:true};
    if(await one(`SELECT "Id" FROM "AbpUsers" WHERE "NormalizedUserName"=$1 AND "TenantId" IS NULL`,[admin.userName.toUpperCase()],db))throw new Error('Admin user already exists; bootstrap does not overwrite passwords');
    return createUser(db,{...admin,tenantId:null,firstName:'Администратор',lastName:'',middleName:'',phone:null,jobPost:'',birthday:null,roles:['admin']});
  });
}
