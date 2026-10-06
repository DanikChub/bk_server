import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript';
export function loadModel(file) {
  const unit = { exports: {} };
  const compiled = ts.transpileModule(fs.readFileSync(file, 'utf8'), { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2021 } }).outputText;
  new Function('require', 'module', 'exports', compiled)(relative => loadModel(path.resolve(path.dirname(file), relative + '.ts')), unit, unit.exports);
  return unit.exports;
}
