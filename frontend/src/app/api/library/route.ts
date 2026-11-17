import type { NextRequest } from 'next/server';
import { resourceRedirect } from '@/server/integrations/resource-route';
import { AppError } from '@/server/errors';
import { errorResponse } from '@/server/http';
export const runtime='nodejs';
export async function GET(request:NextRequest){
  if(!process.env.EXTERNAL_LIBRARY_URL)return errorResponse(new AppError(503,'Внешняя библиотека не настроена'));
  return resourceRedirect(request,process.env.EXTERNAL_LIBRARY_URL,process.env.EXTERNAL_API_KEY??'');
}
