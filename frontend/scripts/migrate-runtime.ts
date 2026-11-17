import { readFile } from 'node:fs/promises';
import { pool } from '../src/server/db';
import path from 'node:path';
async function main() {
  try {
    const sql=await readFile(path.resolve('../database/migrations/004_next_runtime.sql'),'utf8');
    await pool().query(sql);console.log('Runtime migration applied');
  }finally{await pool().end();}
}
void main().catch(error=>{console.error(error.message);process.exitCode=1;});
