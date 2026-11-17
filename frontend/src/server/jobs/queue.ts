import 'server-only';
import { randomUUID } from 'node:crypto';
import { pool, type Executor, type Row } from '../db';
export async function enqueue(db:Executor,kind:string,payload:Row,dedupeKey?:string) {
  const id=randomUUID();
  await db.query(`INSERT INTO "NextJobs"("Id","Kind","Payload","DedupeKey") VALUES($1,$2,$3,$4) ON CONFLICT("DedupeKey") DO NOTHING`,[id,kind,JSON.stringify(payload),dedupeKey??null]);
  return id;
}
export async function queueJob(kind:string,payload:Row,dedupeKey?:string) {return enqueue(pool(),kind,payload,dedupeKey);}
