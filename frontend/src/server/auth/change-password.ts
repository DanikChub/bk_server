import 'server-only';
import { randomUUID } from 'node:crypto';
import { one, update, type Executor } from '../db';
import { hashPassword } from './password';
import { encryptLegacyPassword } from './legacy-encryption';
export async function rotatePassword(db:Executor,userId:string,password:string,actor:string) {
  const stamp=randomUUID();
  await update('AbpUsers',userId,{PasswordHash:await hashPassword(password),SecurityStamp:stamp,LastPasswordChangeTime:new Date(),AccessFailedCount:0,LockoutEnd:null,ShouldChangePasswordOnNextLogin:false},db,actor);
  for(const table of ['CustomerUserProfiles','EmployeeProfiles']) {
    const profile=await one(`SELECT "Id","ExtraProperties" FROM "${table}" WHERE "IdentityUserId"=$1`,[userId],db);
    if(profile){const extra=JSON.parse(String(profile.ExtraProperties||'{}')),encrypted=encryptLegacyPassword(password);if(encrypted)extra.EncryptedPassword=encrypted;else delete extra.EncryptedPassword;await update(table,profile.Id,{ExtraProperties:JSON.stringify(extra)},db,actor);}
  }
  await db.query(`DELETE FROM "NextSessions" WHERE "UserId"=$1`,[userId]);return stamp;
}
