import { readdirSync,readFileSync,writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const root=fileURLToPath(new URL('../../',import.meta.url));
const directory=path.join(root,'KV.Server/src/KV.Server.Application.Contracts/Permissions');
function files(dir){return readdirSync(dir,{withFileTypes:true}).flatMap(item=>item.isDirectory()?files(path.join(dir,item.name)):item.name.endsWith('Permissions.cs')?[path.join(dir,item.name)]:[]);}
const groups=[];
for(const file of files(directory)){
  const constants={};
  for(const [,name,expression] of readFileSync(file,'utf8').matchAll(/public\s+const\s+string\s+(\w+)\s*=\s*([^;]+);/g)){
    constants[name]=expression.trim().split(/\s*\+\s*/).map(part=>{if(part.startsWith('"'))return JSON.parse(part);if(constants[part]!==undefined)return constants[part];throw new Error(`Unresolved ${path.basename(file)}: ${part}`);}).join('');
  }
  if(constants.GroupName)groups.push({group:constants.GroupName,permissions:[...new Set(Object.entries(constants).filter(([name])=>name!=='GroupName').map(([,value])=>value))],source:path.relative(root,file).replaceAll('\\','/')});
}
writeFileSync(path.join(root,'frontend/src/server/auth/permission-catalog.json'),JSON.stringify(groups.sort((a,b)=>a.group.localeCompare(b.group)),null,2)+'\n');
console.log(`Generated ${groups.length} groups and ${groups.reduce((n,g)=>n+g.permissions.length,0)} permission names`);
