import 'server-only';
import { z } from 'zod';
import { assert } from '../errors';
export async function content(kind:string,query:URLSearchParams) {
  assert(kind==='articles'||kind==='news',404,'Раздел не найден');
  assert(process.env.CONTENT_API_URL && process.env.CONTENT_API_AUTHORIZATION,503,'API библиотеки не настроен');
  const page=z.coerce.number().int().min(1).parse(query.get('page')??1),take=z.coerce.number().int().min(1).max(50).parse(query.get('take')??5);
  const url=new URL(kind,process.env.CONTENT_API_URL.endsWith('/')?process.env.CONTENT_API_URL:process.env.CONTENT_API_URL+'/');
  url.searchParams.set('sort[0]',kind==='articles'?'createdAt:desc':'publishedAt:asc');url.searchParams.set('pagination[page]',String(page));url.searchParams.set('pagination[pageSize]',String(take));
  for(const [i,field] of (kind==='articles'?['categories','type_material','seo']:['regions','directions','image','seo']).entries())url.searchParams.set(`populate[${i}]`,field);
  const response=await fetch(url,{headers:{Authorization:process.env.CONTENT_API_AUTHORIZATION},signal:AbortSignal.timeout(30000),cache:'no-store'});
  assert(response.ok,502,'Библиотека временно недоступна');
  const result=await response.json();assert(Array.isArray(result.data),502,'Некорректный ответ библиотеки');
  const items=kind==='articles'?result.data.map((item:Record<string,unknown>)=>{
    const a=(item.attributes??item) as Record<string,unknown>;
    return {Id:item.id,Title:a.title,Text:a.text,Slug:a.slug,Seo:a.seo,StartDate:a.startDate,CreationTime:a.createdAt,LastModificationTime:a.updatedAt,PublishedAt:a.publishedAt};
  }):result.data;
  // Keep the Strapi envelope used by the new UI and the DTO envelope of PostsAppService.
  return {...result,items,totalCount:Number(result.meta?.pagination?.total??items.length)};
}
