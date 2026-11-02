import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const base = fileURLToPath(new URL('../', import.meta.url));
const [replayDb, modelDb] = process.argv.slice(2);
if (!replayDb || !modelDb) throw new Error('Usage: node capture-reference.mjs <diagnostic-replay-db> <original-model-db>');
function read(database) {
  const result = spawnSync(process.env.PSQL ?? 'psql', ['-X', '-At', '-v', 'ON_ERROR_STOP=1', '-d', database, '-f', path.join(base, 'tests/catalog.sql')], { encoding: 'utf8', maxBuffer: 10 * 1024 * 1024 });
  if (result.status !== 0) throw new Error(result.stderr || result.error?.message);
  return JSON.parse(result.stdout.trim());
}
const reference = read(replayDb); const model = read(modelDb);
// Schema/functions are independently derived from replaying the original migrations.
// Explicit HasData rows follow the latest model (one Written type is absent from the historical seed operations).
reference.modelSeeds = model.modelSeeds;
fs.writeFileSync(path.join(base, 'tests/expected-catalog.json'), JSON.stringify(reference, null, 2) + '\n');
const drift = { columns: [], constraints: [] };
for (const actual of reference.columns) {
  const saved = model.columns.find(column => column.table === actual.table && column.name === actual.name);
  if (!saved) throw new Error(`Unexpected migrated column ${actual.table}.${actual.name}`);
  const changes = Object.fromEntries(Object.keys(actual).filter(key => JSON.stringify(actual[key]) !== JSON.stringify(saved[key])).map(key => [key, { migrations: actual[key], model: saved[key] }]));
  if (Object.keys(changes).length) drift.columns.push({ table: actual.table, column: actual.name, changes });
}
for (const actual of reference.constraints) {
  const saved = model.constraints.find(constraint => constraint.table === actual.table && constraint.name === actual.name);
  if (JSON.stringify(actual) !== JSON.stringify(saved)) drift.constraints.push({ migrations: actual, model: saved ?? null });
}
fs.writeFileSync(path.join(base, 'model-migration-differences.json'), JSON.stringify(drift, null, 2) + '\n');
console.log(`Captured reference: ${reference.tables.length} tables, ${reference.columns.length} columns, ${reference.indexes.length} indexes, ${reference.functions.length} functions, ${reference.triggers.length} triggers.`);
console.log(`Model drift: ${drift.columns.length} columns, ${drift.constraints.length} constraints (including raw-SQL-only constraints).`);
