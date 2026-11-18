import { pool, rows, transaction } from '../src/server/db';

// Controlled maintenance after migration/import; never run the baseline on an existing DB.
async function main(){
  try{
    const contracts=await rows(`SELECT "Id" FROM "Contracts" ORDER BY "Id"`);
    for(const contract of contracts)await transaction(db=>db.query('SELECT next_refresh_contract_statistics($1)',[contract.Id]));
    console.log('Contract statistics rebuilt:',contracts.length);
  }finally{await pool().end();}
}
void main().catch(error=>{console.error(error.message);process.exitCode=1;});
