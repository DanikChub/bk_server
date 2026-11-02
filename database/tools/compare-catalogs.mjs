import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const base = fileURLToPath(new URL('../', import.meta.url));
const [expectedDb, actualDb] = process.argv.slice(2);
if (!expectedDb || !actualDb) throw new Error('Usage: node compare-catalogs.mjs <reference-db> <actual-db>');
const binary = process.env.PSQL ?? 'psql';
const catalog = database => {
  const result = spawnSync(binary, ['-X', '-At', '-v', 'ON_ERROR_STOP=1', '-d', database, '-f', path.join(base, 'tests/catalog.sql')], { encoding: 'utf8', maxBuffer: 10 * 1024 * 1024 });
  if (result.status !== 0) throw new Error(result.stderr || result.error?.message);
  return JSON.parse(result.stdout.trim());
};
const expected = catalog(expectedDb); const actual = catalog(actualDb);
const differences = {};
for (const key of Object.keys(expected)) {
  if (JSON.stringify(expected[key]) === JSON.stringify(actual[key])) continue;
  if (Array.isArray(expected[key])) {
    const e = new Set(expected[key].map(row => JSON.stringify(row)));
    const a = new Set(actual[key].map(row => JSON.stringify(row)));
    differences[key] = { onlyReference: [...e].filter(row => !a.has(row)).map(JSON.parse), onlyActual: [...a].filter(row => !e.has(row)).map(JSON.parse) };
  } else differences[key] = { reference: expected[key], actual: actual[key] };
}
fs.mkdirSync(path.join(base, '.local'), { recursive: true });
fs.writeFileSync(path.join(base, '.local/catalog-differences.json'), JSON.stringify(differences, null, 2) + '\n');
console.log(JSON.stringify(Object.fromEntries(Object.entries(differences).map(([key, value]) => [key, { onlyReference: value.onlyReference?.length, onlyActual: value.onlyActual?.length }]))));
console.log(`Full differences: ${path.join(base, '.local/catalog-differences.json')}`);
