import { api } from '@/server/http';
export const GET = api(async(_req,session)=>session);
