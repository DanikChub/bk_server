import { readFile } from 'node:fs/promises';
import { pool } from '../src/server/db';
import path from 'node:path';
async function main() {
  try {
    for(const name of ['004_next_runtime.sql','005_runtime_statistics.sql','006_account_recovery.sql']){
      const sql=await readFile(path.resolve('../database/migrations',name),'utf8');
      await pool().query(sql);console.log('Runtime migration applied:',name);
    }
  }finally{await pool().end();}
}
void main().catch(error=>{console.error(error.message);process.exitCode=1;});
