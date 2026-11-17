import { bootstrap } from '../src/server/bootstrap';
import { pool } from '../src/server/db';
async function main() {
  try {
  const password=process.env.BOOTSTRAP_ADMIN_PASSWORD;
  if(password && password.length<12)throw new Error('BOOTSTRAP_ADMIN_PASSWORD must contain at least 12 characters');
  const result=await bootstrap(password?{userName:process.env.BOOTSTRAP_ADMIN_USERNAME??'admin',email:process.env.BOOTSTRAP_ADMIN_EMAIL??'admin@localhost.local',password}:undefined);
  console.log('Bootstrap completed',result);
  }finally{await pool().end();}
}
void main().catch(error=>{console.error(error.message);process.exitCode=1;});
