import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { loadModel } from './helpers/load-model.mjs';
const store=loadModel(fileURLToPath(new URL('../src/shared/lib/stored-value-store.ts',import.meta.url)));

test('Blocked storage still synchronizes page and header and keeps different users separate',()=>{
 const previous=globalThis.window;
 const window=new EventTarget();window.localStorage={getItem(){throw Error('blocked');},setItem(){throw Error('blocked');}};
 globalThis.window=window;
 let page,header;
 const unPage=store.subscribeStoredValue(()=>{page=store.readStoredSnapshot('user-a');});
 const unHeader=store.subscribeStoredValue(()=>{header=store.readStoredSnapshot('user-a');});
 try {
  assert.equal(store.writeStoredValue('user-a','{"read":true}'),false);
  assert.equal(page,'{"read":true}');assert.equal(header,page);
  assert.equal(store.readStoredSnapshot('user-b'),null);
  const values=new Map();window.localStorage={getItem:key=>values.get(key)??null,setItem:(key,value)=>values.set(key,value)};
  assert.equal(store.writeStoredValue('user-a','{"read":false}'),true);
  assert.equal(header,'{"read":false}');assert.equal(store.readStoredSnapshot('user-a'),header);
  values.set('user-a','{"fromAnotherTab":true}');
  const event=new Event('storage');Object.defineProperty(event,'key',{value:'user-a'});window.dispatchEvent(event);
  assert.equal(page,'{"fromAnotherTab":true}');assert.equal(header,page);
  unPage();unHeader();store.writeStoredValue('user-a','{}');assert.equal(header,'{"fromAnotherTab":true}');
 } finally {unPage();unHeader();globalThis.window=previous;}
});
