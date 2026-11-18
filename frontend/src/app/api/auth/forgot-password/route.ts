import { NextRequest } from 'next/server';
import { checkOrigin,errorResponse,jsonBody } from '@/server/http';
import { requestPasswordReset } from '@/server/auth/recovery';
export const runtime='nodejs';
export async function POST(request:NextRequest){try{checkOrigin(request);return Response.json(await requestPasswordReset(await jsonBody(request,16384)),{headers:{'Cache-Control':'no-store'}});}catch(error){return errorResponse(error);}}
