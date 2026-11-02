import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../../', import.meta.url));
const base = path.join(root, 'database');
if (!process.env.PGDATABASE) throw new Error('Set PGDATABASE to an initialized disposable test database; see database/README.md.');
const run = (file, extra = []) => spawnSync(process.env.PSQL ?? 'psql', ['-X', '-v', 'ON_ERROR_STOP=1', ...extra, '-f', path.join(base, file)], { encoding: 'utf8', maxBuffer: 10 * 1024 * 1024 });
const readCatalog = () => {
  const result = run('tests/catalog.sql', ['-At']);
  assert.equal(result.status, 0, result.stderr || result.error?.message);
  return JSON.parse(result.stdout.trim());
};
const manifest = JSON.parse(fs.readFileSync(path.join(base, 'source-manifest.json'), 'utf8'));
for (const source of manifest.sourceFiles) {
  const content = fs.readFileSync(path.join(root, source.path), 'utf8').replace(/\r\n/g, '\n').trimEnd() + '\n';
  const hash = crypto.createHash('sha256').update(content).digest('hex');
  assert.equal(hash, source.sha256, `Legacy source changed: ${source.path}. Regenerate and review the baseline.`);
}
console.log(`PASS: ${manifest.sourceFiles.length} original migration/model sources match their recorded hashes.`);

const expected = JSON.parse(fs.readFileSync(path.join(base, 'tests/expected-catalog.json'), 'utf8'));
const before = readCatalog();
for (const key of Object.keys(expected)) assert.deepEqual(before[key], expected[key], `PostgreSQL catalog mismatch: ${key}`);
console.log(`PASS: full reference catalog (${before.tables.length} tables, ${before.columns.length} columns, ${before.indexes.length} indexes, ${before.functions.length} functions, ${before.triggers.length} triggers).`);

const behavior = run('tests/behavior.sql');
assert.equal(behavior.status, 0, behavior.stderr || behavior.error?.message);
assert.match(behavior.stderr, /All legacy schema and trigger behavior assertions passed/);
assert.deepEqual(readCatalog(), before, 'Behavior fixtures must roll back completely.');
const assertionCount = (fs.readFileSync(path.join(base, 'tests/behavior.sql'), 'utf8').match(/^\s*ASSERT /gm) ?? []).length;
console.log(`PASS: ${assertionCount} behavior assertions, including original trigger defects; fixtures rolled back.`);

const repeatedInit = run('init.sql');
assert.notEqual(repeatedInit.status, 0, 'Initializer must reject a nonempty schema.');
assert.match(repeatedInit.stderr, /initialization requires an empty public schema/);
assert.deepEqual(readCatalog(), before, 'Rejected initialization must not modify the database.');
console.log('PASS: repeated initialization is rejected without changing schema or reference data.');
