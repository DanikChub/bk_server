import 'server-only';
import { z } from 'zod';
import { assert } from '../errors';
type Item=Record<string,unknown>;
function coverUrl(thumbnail:unknown){
  const image=thumbnail as {items?:{_pieces?:{item?:{attachment?:{_urls?:{full?:unknown}}}}[]}[]} | undefined;
  const url=image?.items?.[0]?._pieces?.[0]?.item?.attachment?._urls?.full;
  return typeof url==='string'?url:null;
}
export async function legacyContent(kind:string,query:URLSearchParams) {
  assert(kind==='articles'||kind==='news',404,'Раздел не найден');
  const key=process.env[kind==='articles'?'BK_ARTICLE_KEY':'BK_NEWS_KEY'];assert(key,503,'Старое API библиотеки не настроено');
  const take=z.coerce.number().int().min(1).max(50).parse(query.get('take')??30),skip=z.coerce.number().int().min(0).parse(query.get('skip')??0);
  const url=new URL(kind==='articles'?'apostrophe-blog':'news',process.env.BK_API_URL??'https://bk.kv34.ru/api/v1/');
  url.searchParams.set('apikey',key);url.searchParams.set('includeFields',kind==='articles'?'title,postType,publishedAt':'title,thumbnail,searchSummary,_purpose,publishedAt');url.searchParams.set('published','true');url.searchParams.set('perPage',String(take));url.searchParams.set('page',String(Math.floor(skip/take)+1));
  const response=await fetch(url,{signal:AbortSignal.timeout(30000),cache:'no-store'});assert(response.ok,502,'Старое API библиотеки недоступно');
  const payload=await response.json();assert(Array.isArray(payload.results),502,'Некорректный ответ библиотеки');
  const items=(payload.results as Item[]).map(item=>({Name:item.title,Url:item._url,PublishedAt:item.publishedAt,...(kind==='articles'?{Type:item.postType}:{ShortContent:item.searchSummary,Purpose:Array.isArray(item._purpose)?item._purpose.map(p=>(p as Item).title):[],CoverUrls:coverUrl(item.thumbnail),Thumbnail:item.thumbnail})}));
  return {totalCount:payload.total,items};
}
