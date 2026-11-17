import type { NextRequest } from 'next/server';
import { resourceRedirect } from '@/server/integrations/resource-route';
export const runtime='nodejs';
export async function GET(request:NextRequest){return resourceRedirect(request,request.nextUrl.searchParams.get('source')??'',request.nextUrl.searchParams.get('resId')??'');}
