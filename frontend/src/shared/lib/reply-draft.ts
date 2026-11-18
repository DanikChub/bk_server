type Draft={text:string;files:File[]};
function database():Promise<IDBDatabase>{return new Promise((resolve,reject)=>{const request=indexedDB.open('consulting-reply-drafts',1);request.onupgradeneeded=()=>{if(!request.result.objectStoreNames.contains('drafts'))request.result.createObjectStore('drafts');};request.onsuccess=()=>resolve(request.result);request.onerror=()=>reject(new Error('Хранилище черновиков недоступно'));});}
export async function replyDraft(key:string,value?:Draft|null):Promise<Draft|undefined>{
  const db=await database();try{return await new Promise((resolve,reject)=>{const tx=db.transaction('drafts',value===undefined?'readonly':'readwrite'),store=tx.objectStore('drafts');let found:Draft|undefined;
    const request=value===undefined?store.get(key):value===null?store.delete(key):store.put(value,key);request.onsuccess=()=>{if(value===undefined)found=request.result as Draft|undefined;};request.onerror=()=>reject(new Error('Не удалось сохранить черновик'));tx.oncomplete=()=>resolve(found);tx.onerror=()=>reject(new Error('Не удалось сохранить черновик'));tx.onabort=()=>reject(new Error('Не удалось сохранить черновик'));
  });}finally{db.close();}
}
