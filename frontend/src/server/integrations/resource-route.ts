import 'server-only';
import type { NextRequest } from 'next/server';
import { loadSession } from '../auth/session';
import { externalResource } from './external-auth';
import { AppError } from '../errors';
import { errorResponse } from '../http';
export async function resourceRedirect(request:NextRequest,source:string,resourceId:string) {
  try {
    const session=await loadSession();
    return new Response(null,{status:302,headers:{Location:externalResource(session,source,resourceId),'Cache-Control':'no-store','Referrer-Policy':'no-referrer'}});
  }catch(error){
    if(error instanceof AppError && error.status===401){
      const login=new URL('/client/login',request.url);login.searchParams.set('returnTo',request.nextUrl.pathname+request.nextUrl.search);
      return new Response(null,{status:302,headers:{Location:login.toString(),'Cache-Control':'no-store'}});
    }
    return errorResponse(error);
  }
}
