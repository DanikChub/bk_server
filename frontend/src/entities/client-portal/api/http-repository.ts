import type { PortalRepository, PortalSnapshot, PortalTicket, SaveQuestion } from '../model/types';

export async function backend<T>(path:string,method='GET',body?:unknown,signal?:AbortSignal):Promise<T> {
  const response=await fetch('/api/v1/'+path,{method,credentials:'same-origin',cache:'no-store',signal,headers:body instanceof FormData?undefined:body===undefined?undefined:{'Content-Type':'application/json'},body:body instanceof FormData?body:body===undefined?undefined:JSON.stringify(body)});
  if(!response.ok){const result=await response.json().catch(()=>null);throw new Error(result?.error?.message??'Сервер недоступен');}
  return response.json();
}
let snapshot:PortalSnapshot|null=null;
const materialCache=new Map<string,{expires:number;value:Promise<PromiseSettledResult<{data:Record<string,unknown>[]}>[]>}>();
async function load(signal?:AbortSignal,options?:{includeContent?:boolean}){
  const value=await backend<PortalSnapshot>('portal','GET',undefined,signal);
  if(options?.includeContent===false){snapshot=value;return value;}
  let cached=materialCache.get(value.profile.id);
  if(!cached||cached.expires<Date.now()){
    const timeout=AbortSignal.timeout(5000);
    cached={expires:Date.now()+300_000,value:Promise.allSettled(['articles','news'].map(kind=>backend<{data:Record<string,unknown>[]}>(`content/${kind}?take=50`,'GET',undefined,timeout)))};
    materialCache.set(value.profile.id,cached);if(materialCache.size>10)materialCache.delete(materialCache.keys().next().value!);
  }
  const results=await cached.value;if(signal?.aborted)throw signal.reason;
  if(results.some(r=>r.status==='rejected'))cached.expires=Math.min(cached.expires,Date.now()+30_000);
  const contentRows=(index:number)=>{const result=results[index];return result.status==='fulfilled'&&Array.isArray(result.value.data)?result.value.data:[];};
  const map=(item:Record<string,unknown>,kind:string)=>{const attrs=(item.attributes??item) as Record<string,unknown>;return {id:kind+'-'+String(item.id),title:String(attrs.title??''),body:String(attrs.text??attrs.body??''),icon:'a9517.png'};};
  value.content={articles:contentRows(0).map(item=>map(item,'article')),news:contentRows(1).map(item=>map(item,'news'))};
  if(results.some(r=>r.status==='rejected'))value.contentError='Материалы библиотеки временно недоступны.';
  snapshot=value;return value;
}
async function ticket(id:string):Promise<PortalTicket>{const value=(await load(undefined,{includeContent:false})).tickets.find(t=>t.id===id);if(!value)throw new Error('Заявка не найдена');return value;}
export async function command<T>(path:string,data:unknown,files:File[]):Promise<T>{
  if(!files.length)return backend<T>(path,'POST',data);
  const form=new FormData();form.set('data',JSON.stringify(data));for(const file of files)form.append('files',file);
  return backend<T>(path,'POST',form);
}
async function question(input:SaveQuestion){
  const types=await backend<{items:{Id:string;Name:string}[]}>('catalogs/ticket-types');
  const sections=await backend<{items:{Id:string;Name:string}[]}>('catalogs/ticket-sections');
  const type=types.items.find(t=>t.Name==='Письменная')??types.items[0];if(!type)throw new Error('Типы заявок не настроены');
  const section=sections.items.find(t=>t.Name===input.section || t.Id===input.section);
  const body={subject:input.subject,description:input.description,ticketTypeId:type.Id,ticketSectionId:section?.Id??null,draft:input.draft,...(input.id?{retainedAttachmentIds:input.existingAttachments.map(f=>f.id)}:{})};
  const result=await command<{Id:string}>(input.id?`tickets/${input.id}/draft`:'tickets',body,input.files);
  return ticket(String(result.Id));
}
export const httpPortalRepository:PortalRepository={
  load,saveQuestion:question,
  async reply(id,text,files){await command(`tickets/${id}/reply`,{description:text},files);return ticket(id);},
  async setFavorite(id,favorite){await backend(`tickets/${id}/favorite`,'POST',{favorite});return ticket(id);},
  async rate(id,rating){await backend(`tickets/${id}/rating`,'POST',{rating});return ticket(id);},
  async updateProfile(input){return backend('portal/profile','PATCH',input);},
  async updateAvatar(file){const form=new FormData();form.set('file',file);await backend('profile/avatar','POST',form);return (await load()).profile;},
  async setEventCompleted(id,completed){return backend(`portal/events/${id}`,'POST',{completed});},
  async requestRenewal(contractId,description,files){await command('portal/renewals',{contractId,description},files);await load();},
  async downloadAttachment(id){
    const data=snapshot??await load();const owner=data.tickets.find(t=>t.attachments.some(f=>f.id===id)||t.messages.some(m=>m.attachments.some(f=>f.id===id)));
    if(!owner)throw new Error('Файл недоступен');
    const response=await fetch(`/api/v1/tickets/${owner.id}/files/${id}`,{credentials:'same-origin'});if(!response.ok)throw new Error('Не удалось скачать файл');return response.blob();
  },
};
