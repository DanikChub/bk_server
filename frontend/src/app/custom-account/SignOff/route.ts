import type { NextRequest } from 'next/server';
import { logout } from '@/server/auth/session';
import { assert } from '@/server/errors';
import { errorResponse } from '@/server/http';
export const runtime='nodejs';
export async function GET(request:NextRequest){
  try {
    const origin=new URL(process.env.APP_URL??request.url).origin,site=request.headers.get('sec-fetch-site'),referrer=request.headers.get('referer');
    assert(site==='same-origin'||site==='none'||Boolean(referrer&&new URL(referrer).origin===origin),403,'Недопустимый источник выхода');
    await logout();
    const target=request.nextUrl.searchParams.get('returnUrl')??'/client/login';
    const url=new URL(target,origin);assert(url.origin===origin && !target.includes('\\'),400,'Недопустимый адрес возврата');
    const hash=request.nextUrl.searchParams.get('returnUrlHash');if(hash)url.hash=hash;
    return new Response(null,{status:302,headers:{Location:url.toString(),'Cache-Control':'no-store'}});
  }catch(error){return errorResponse(error);}
}
