import 'server-only';
import { NextRequest, NextResponse } from 'next/server';
import { ZodError } from 'zod';
import { AppError, assert } from './errors';
import { loadSession, type Session } from './auth/session';
import { randomUUID } from 'node:crypto';
import { auditContext } from './audit-context';

export function checkOrigin(request: NextRequest) {
  if (['GET','HEAD','OPTIONS'].includes(request.method)) return;
  if(process.env.NODE_ENV==='production')assert(process.env.APP_URL,503,'Адрес приложения не настроен');
  const expected = new URL(process.env.APP_URL ?? request.url).origin;
  assert(request.headers.get('origin') === expected,403,'Недопустимый источник запроса');
}
export async function jsonBody(request:Request,limit=2_000_000):Promise<unknown> {
  assert(Number(request.headers.get('content-length')??0)<=limit,413,'Слишком большой запрос');
  const reader=request.body?.getReader();assert(reader,400,'Пустой запрос');const chunks:Uint8Array[]=[];let size=0;
  try{for(;;){const part=await reader.read();if(part.done)break;size+=part.value.byteLength;assert(size<=limit,413,'Слишком большой запрос');chunks.push(part.value);}}
  catch(error){await reader.cancel();throw error;}
  try{return JSON.parse(Buffer.concat(chunks).toString('utf8'));}catch{throw new AppError(400,'Некорректный JSON');}
}
export function api(fn: (req: NextRequest, session: Session) => Promise<unknown>) {
  return async (request: NextRequest) => {
    try {
      checkOrigin(request);
      const session = await loadSession();
      const result = await auditContext.run({userId:session.userId,tenantId:session.tenantId,method:request.method,path:request.nextUrl.pathname,requestId:randomUUID()},()=>fn(request,session));
      if (result instanceof Response) return result;
      return NextResponse.json(result, { headers: { 'Cache-Control':'no-store' } });
    } catch (error) { return errorResponse(error); }
  };
}
export function errorResponse(error: unknown) {
  if (error instanceof ZodError) return NextResponse.json({ error: { code:'validation_error',message:'Проверьте данные запроса',details:error.flatten() } },{ status:400 });
  if (error instanceof AppError) return NextResponse.json({ error:{ code:error.code,message:error.message } },{ status:error.status });
  const code = (error as {code?:string})?.code;
  if (code === '23505') return NextResponse.json({error:{code:'conflict',message:'Запись уже существует'}},{status:409});
  if (code === '23503') return NextResponse.json({error:{code:'reference_conflict',message:'Связанная запись отсутствует или используется'}},{status:409});
  console.error('API request failed', { name:(error as Error)?.name, code });
  return NextResponse.json({error:{code:'internal_error',message:'Не удалось выполнить операцию'}},{status:500});
}
