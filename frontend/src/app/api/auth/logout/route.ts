import { api } from '@/server/http';
import { logout } from '@/server/auth/session';
export const POST = api(async()=>{ await logout(); return {ok:true}; });
