import { readdirSync, readFileSync, mkdirSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const root=fileURLToPath(new URL('../../',import.meta.url));
function files(dir){return readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?['obj','bin','wwwroot'].includes(e.name)?[]:files(path.join(dir,e.name)):e.name.endsWith('.cs')&&!e.name.endsWith('.Designer.cs')?[path.join(dir,e.name)]:[]);}
const areas=['KV.Server.Application','KV.Server.Domain','KV.Server.Web/Pages','KV.Server.PublicWeb/Pages','KV.Server.HttpApi/Controllers','KV.Server.Migration'];
const operations=[];
for(const area of areas)for(const file of files(path.join(root,'KV.Server/src',area))){
  const source=readFileSync(file,'utf8');
  for(const match of source.matchAll(/^\s*public\s+(?:(?:override|virtual|async|static|sealed|new)\s+)*[^\n;={}]+?\s+([A-Za-z_]\w*)\s*\(/gm)){
    const line=source.slice(0,match.index).split('\n').length;
    operations.push({source:path.relative(root,file).replaceAll('\\','/'),line,method:match[1],status:'requires-parity-review'});
  }
}
mkdirSync(path.join(root,'docs'),{recursive:true});
writeFileSync(path.join(root,'docs/legacy-operations.json'),JSON.stringify({description:'Source inventory, not a claim of completed parity. Inherited ABP APIs, Razor/JS conditions and private workflows also require review.',operations},null,2)+'\n');
console.log(`Inventoried ${operations.length} declared public methods`);
