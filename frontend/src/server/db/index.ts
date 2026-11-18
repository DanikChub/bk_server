import 'server-only';
import { Pool, type PoolClient, type QueryResultRow } from 'pg';
import { drizzle } from 'drizzle-orm/node-postgres';
import * as schema from './legacy-schema';
import catalog from './legacy-catalog.json';
import { randomUUID } from 'node:crypto';
import { auditContext } from '../audit-context';
import { AsyncLocalStorage } from 'node:async_hooks';

const globals = globalThis as unknown as { legacyPool?: Pool };
export function pool(): Pool {
  if (!process.env.DATABASE_URL) throw new Error('DATABASE_URL is required');
  return globals.legacyPool ??= new Pool({ connectionString: process.env.DATABASE_URL, max: 12, connectionTimeoutMillis: 5000 });
}
export function orm() { return drizzle(pool(), { schema }); }
export type Executor = Pick<PoolClient, 'query'>;
export type Row = Record<string, unknown>;
const transactionContext=new AsyncLocalStorage<{db:PoolClient;rollbackHooks:(()=>Promise<void>)[]}>();
export function executor():Executor{return transactionContext.getStore()?.db??pool();}
export function onRollback(fn:()=>Promise<void>){const context=transactionContext.getStore();if(!context)throw new Error('Rollback hook requires a transaction');context.rollbackHooks.push(fn);}
export async function rows<T extends QueryResultRow = Row>(text: string, values: unknown[] = [], db: Executor = executor()): Promise<T[]> {
  return (await db.query<T>(text, values)).rows;
}
export async function one<T extends QueryResultRow = Row>(text: string, values: unknown[] = [], db: Executor = executor()): Promise<T | undefined> {
  return (await rows<T>(text, values, db))[0];
}
export async function transaction<T>(fn:(db:PoolClient)=>Promise<T>):Promise<T>{
  const parent=transactionContext.getStore();if(parent)return fn(parent.db);
  for(let attempt=0;;attempt++){
    try{return await transactionAttempt(fn);}catch(error){
      const code=(error as {code?:string}).code;
      if(attempt>=2||!['40001','40P01'].includes(code??''))throw error;
      await new Promise(resolve=>setTimeout(resolve,20*(attempt+1)));
    }
  }
}
async function transactionAttempt<T>(fn: (db: PoolClient) => Promise<T>): Promise<T> {
  const parent=transactionContext.getStore();if(parent)return fn(parent.db);
  const db = await pool().connect();
  const state={db,rollbackHooks:[] as (()=>Promise<void>)[]};let released=false;
  try {
    await db.query('BEGIN'); const result = await transactionContext.run(state,()=>fn(db));
    const audit=auditContext.getStore();
    if(audit && !['GET','HEAD','OPTIONS'].includes(audit.method))await db.query(`INSERT INTO "NextAuditLog"("Id","UserId","TenantId","Method","Path","RequestId") VALUES($1,$2,$3,$4,$5,$6)`,[randomUUID(),audit.userId,audit.tenantId,audit.method,audit.path,audit.requestId]);
    await db.query('COMMIT'); return result;
  }
  catch (e) {
    try{await db.query('ROLLBACK');}catch{}
    // Release before compensations: their fresh queries must not deadlock a full pool.
    db.release();released=true;
    for(const hook of state.rollbackHooks.reverse())try{await hook();}catch{console.error('Rollback compensation requires operator review');}
    throw e;
  }
  finally { if(!released)db.release(); }
}
export function identifier(name: string): string { return '"' + name.replaceAll('"', '""') + '"'; }
export function columns(table: string) { return catalog.columns.filter(c => c.table === table); }
export async function insert(table: string, input: Row, db: Executor, actor?: string): Promise<Row> {
  const fields = columns(table);
  if (!fields.length) throw new Error(`Unknown table ${table}`);
  const values: Row = { ...input };
  const has = (name: string) => fields.some(c => c.name === name);
  if (has('Id') && !values.Id && fields.find(c => c.name === 'Id')?.type === 'uuid') values.Id = randomUUID();
  if (has('CreationTime') && !values.CreationTime) values.CreationTime = new Date();
  if (has('CreatorId') && actor && !values.CreatorId) values.CreatorId = actor;
  if (has('ConcurrencyStamp') && !values.ConcurrencyStamp) values.ConcurrencyStamp = randomUUID();
  if (has('ExtraProperties') && values.ExtraProperties === undefined) values.ExtraProperties = '{}';
  if (has('IsDeleted') && values.IsDeleted === undefined) values.IsDeleted = false;
  for (const key of Object.keys(values)) if (!has(key)) throw new Error(`Unknown column ${table}.${key}`);
  const keys = Object.keys(values).filter(k => values[k] !== undefined);
  const result = await one(`INSERT INTO ${identifier(table)} (${keys.map(identifier)}) VALUES (${keys.map((_,i) => '$'+(i+1))}) RETURNING *`, keys.map(k=>values[k]), db);
  return result!;
}
export async function update(table: string, id: unknown, input: Row, db: Executor, actor?: string): Promise<Row | undefined> {
  const fields = columns(table);
  const values = { ...input };
  if (fields.some(c=>c.name === 'LastModificationTime')) values.LastModificationTime = new Date();
  if (actor && fields.some(c=>c.name === 'LastModifierId')) values.LastModifierId = actor;
  if (fields.some(c=>c.name === 'ConcurrencyStamp')) values.ConcurrencyStamp = randomUUID();
  const keys = Object.keys(values);
  if (!keys.length || keys.some(k => k === 'Id' || !fields.some(c=>c.name === k))) throw new Error('Invalid update fields');
  return one(`UPDATE ${identifier(table)} SET ${keys.map((k,i)=>`${identifier(k)}=$${i+1}`)} WHERE "Id"=$${keys.length+1} RETURNING *`, [...keys.map(k=>values[k]), id], db);
}
