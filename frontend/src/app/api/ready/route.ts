import { one } from '@/server/db';
export const runtime='nodejs';
export const dynamic='force-dynamic';
export async function GET(){
  try{
    const result=await one(`SELECT to_regclass('public."Tickets"') IS NOT NULL AND to_regclass('public."NextSessions"') IS NOT NULL AND to_regclass('public."NextAuditLog"') IS NOT NULL AS ready`);
    return Response.json({status:result?.ready?'ready':'not-ready'},{status:result?.ready?200:503,headers:{'Cache-Control':'no-store'}});
  }catch{return Response.json({status:'not-ready'},{status:503,headers:{'Cache-Control':'no-store'}});}
}
