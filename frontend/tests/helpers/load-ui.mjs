import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import ts from 'typescript';
const require = createRequire(import.meta.url);
const src = path.resolve(path.dirname(new URL(import.meta.url).pathname), '../../src');
const cache = new Map();
export function loadUI(file) {
 if (cache.has(file)) return cache.get(file).exports;
 const unit = {exports:{}};cache.set(file,unit);
 const code = ts.transpileModule(fs.readFileSync(file,'utf8'), {fileName:file,compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2021,jsx:ts.JsxEmit.ReactJSX}}).outputText;
 const resolve = specifier => {
  if (specifier.endsWith('.css')) return {};
  if (!specifier.startsWith('.') && !specifier.startsWith('@/')) return require(specifier);
  const base = specifier.startsWith('@/') ? path.join(src,specifier.slice(2)) : path.resolve(path.dirname(file),specifier);
  const target = [base,base+'.ts',base+'.tsx'].find(candidate=>fs.existsSync(candidate)&&fs.statSync(candidate).isFile());
  if(!target)throw new Error(`Unresolved test import ${specifier} from ${file}`);
  return loadUI(target);
 };
 new Function('require','module','exports',code)(resolve,unit,unit.exports);
 return unit.exports;
}
