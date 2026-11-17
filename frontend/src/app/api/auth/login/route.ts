import { NextRequest, NextResponse } from 'next/server';
import { z } from 'zod';
import { cookies } from 'next/headers';
import { login, sessionCookie } from '@/server/auth/session';
import { checkOrigin, errorResponse, jsonBody } from '@/server/http';
export const runtime = 'nodejs';
export async function POST(req: NextRequest) {
  try {
    checkOrigin(req);
    const input = z.object({userName:z.string().trim().min(1).max(256),password:z.string().min(1).max(1024),tenant:z.string().trim().min(1).max(64).nullable().default(null),authenticatorCode:z.string().regex(/^\d{6}$/).optional(),newPassword:z.string().min(8).max(1024).optional()}).strict().parse(await jsonBody(req,16384));
    const token = await login(input.userName,input.password,input.tenant,'login',input.authenticatorCode,input.newPassword);
    (await cookies()).set(sessionCookie,token,{httpOnly:true,secure:process.env.NODE_ENV==='production',sameSite:'lax',path:'/',maxAge:12*60*60});
    return NextResponse.json({ok:true},{headers:{'Cache-Control':'no-store'}});
  } catch (e) { return errorResponse(e); }
}
