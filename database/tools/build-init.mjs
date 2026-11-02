import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../../', import.meta.url));
const base = path.join(root, 'database');
const input = path.join(base, '.local/export');
const lf = value => value.replace(/\r\n/g, '\n').trimEnd() + '\n';
if (process.argv.includes('--prepare-replay')) {
  const replay = fs.readFileSync(path.join(input, 'replay.sql'), 'utf8');
  const missingTransition = /ALTER TABLE "ContractSettings" (DISABLE|ENABLE) TRIGGER upsert_constraint_monthly_stats_trigger;/g;
  const matches = [...replay.matchAll(missingTransition)];
  if (matches.length !== 2) throw new Error(`Expected exactly two documented missing-trigger transitions, got ${matches.length}.`);
  fs.writeFileSync(path.join(input, 'replay-audit.sql'), lf(replay.replace(missingTransition,
    '-- Source gap: temporary transition for an undefined ContractSettings trigger; no final object is invented.')));
  console.log('Prepared diagnostic replay; only the two documented undefined-trigger transitions were omitted.');
  process.exit(0);
}
const snapshot = lf(fs.readFileSync(path.join(input, 'snapshot.sql'), 'utf8'));
const dump = lf(fs.readFileSync(path.join(input, 'replayed-schema.sql'), 'utf8'));
const operations = JSON.parse(fs.readFileSync(path.join(input, 'raw-operations.json'), 'utf8'));
const functions = new Map();
const triggers = new Map();
const constraints = new Map();
const transient = [];
for (const operation of operations) {
  const sql = lf(operation.sql).trim();
  let match;
  if ((match = sql.match(/^create\s+(?:or\s+replace\s+)?function\s+(?:public\.)?(\w+)\s*\(/i))) {
    functions.set(match[1], { source: operation.migration, sql });
  } else if ((match = sql.match(/^create\s+trigger\s+(\w+)[\s\S]*?\bon\s+(?:public\.)?"([^"]+)"/i))) {
    triggers.set(`${match[2]}.${match[1]}`, { source: operation.migration, sql });
  } else if ((match = sql.match(/^drop\s+trigger\s+if\s+exists\s+(\w+)\s+on\s+(?:public\.)?"([^"]+)"/i))) {
    triggers.delete(`${match[2]}.${match[1]}`);
  } else if ((match = sql.match(/^alter\s+table\s+(?:public\.)?"([^"]+)"\s+add\s+constraint\s+(\w+)\s+unique/i))) {
    constraints.set(`${match[1]}.${match[2]}`, { source: operation.migration, sql });
  } else if (/^alter\s+table\s+/i.test(sql) && /\b(?:disable|enable)\s+trigger\b/i.test(sql)) {
    // These are temporary migration steps, not final schema objects. In particular,
    // UserMails_Part3 refers to an undefined ContractSettings trigger; see README.
    transient.push({ source: operation.migration, sql });
  } else {
    throw new Error(`Unclassified raw SQL in ${operation.migration}; inspect it before generating an initializer.`);
  }
}

const render = (title, entries) => `-- ${title}\n\n` + [...entries].map(([name, operation]) =>
  `-- ${name}\n-- Source: KV.Server/.../Migrations/${operation.source}.cs (Up)\n${operation.sql}\n`).join('\n');
// Use the final *database* schema, not just the saved EF model. Historical migrations
// leave DB defaults, five timestamptz columns and one FK action absent from that model.
// pg_dump sections are independently typed: retain every structural section and fail
// on unexpected types instead of guessing which objects can be omitted.
const sections = dump.split(/(?=^--\n-- Name: )/m).slice(1);
const structuralTypes = new Set(['TABLE', 'SEQUENCE', 'SEQUENCE OWNED BY', 'DEFAULT', 'CONSTRAINT', 'INDEX', 'FK CONSTRAINT', 'TABLE ATTACH']);
const structural = [];
for (const section of sections) {
  // pg_dump table patterns fold unquoted mixed-case identifiers. Explicitly exclude
  // only EF bookkeeping TOC entries as well, so the fresh baseline has no fake history.
  if (section.split('\n')[1].includes('__EFMigrationsHistory')) continue;
  const type = section.match(/^--\n-- Name: .*?; Type: ([^;]+); Schema: /)?.[1];
  if (type === 'FUNCTION' || type === 'TRIGGER') continue;
  if (!structuralTypes.has(type)) throw new Error(`Unclassified pg_dump section: ${type}`);
  const cleaned = section.replace(/^\\(?:un)?restrict .*\n/gm, '').replace(/^-- PostgreSQL database dump complete\n/gm, '');
  structural.push(cleaned);
}
const schema = '-- Final PostgreSQL structure after the original migration operations.\n' +
  '-- Includes migration-only defaults, FK actions, sequences and raw unique constraints.\n' +
  '-- See README for the two undefined-trigger transitions in the historical migration chain.\n\n' + structural.join('\n');
const modelTableCount = (snapshot.match(/^CREATE TABLE /gm) ?? []).length;
const actualTableCount = (schema.match(/^CREATE TABLE /gm) ?? []).length;
if (actualTableCount !== modelTableCount) throw new Error(`Expected ${modelTableCount} tables, got ${actualTableCount}.`);
const sqlObjects = render('Last original definition of each PostgreSQL function. Bodies are unchanged apart from line endings.', functions) + '\n' +
  render('Final active triggers, after applying original CREATE/DROP operations in migration order.', triggers);
const seedStatements = snapshot.match(/^INSERT INTO [\s\S]*?;$/gm) ?? [];
const seeds = '-- Explicit HasData reference rows from the latest saved model. No customer data or generated account IDs.\n\n' + seedStatements.join('\n\n') + '\n';
const header = `-- KV.Server PostgreSQL initialization. SQL only; .NET/Prisma are not required to run this file.\n` +
  `-- Sources: final SQL effects of 67 original migrations through 20240826092427 + explicit model seed rows.\n` +
  `-- Generated by database/tools/build-init.mjs. Do not edit generated SQL by hand.\n` +
  `-- Original trigger behavior (including known defects) is deliberately preserved. See database/README.md.\n` +
  `-- Apply only to a new empty database; no customer data or credentials are included.\n\n` +
  `BEGIN;\nSET LOCAL search_path = public;\n\n` +
  `DO $bootstrap_guard$\nBEGIN\n` +
  `    IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace\n` +
  `               WHERE n.nspname = 'public' AND c.relkind IN ('r', 'p', 'S', 'v', 'm', 'f')) THEN\n` +
  `        RAISE EXCEPTION 'KV.Server initialization requires an empty public schema';\n` +
  `    END IF;\nEND;\n$bootstrap_guard$;\n\n`;
fs.mkdirSync(path.join(base, 'migrations'), { recursive: true });
fs.writeFileSync(path.join(base, 'migrations/001_legacy_schema.sql'), lf(schema));
fs.writeFileSync(path.join(base, 'migrations/002_legacy_sql_objects.sql'), lf(sqlObjects));
fs.writeFileSync(path.join(base, 'migrations/003_model_reference_data.sql'), lf(seeds));
fs.writeFileSync(path.join(base, 'init.sql'), lf(header + schema + '\n' + sqlObjects + '\n' + seeds + '\nCOMMIT;\n'));

const migrationDirectory = path.join(root, 'KV.Server/src/KV.Server.EntityFrameworkCore/Migrations');
const sourceFiles = fs.readdirSync(migrationDirectory).filter(file => file.endsWith('.cs')).sort().map(file => ({
  path: `KV.Server/src/KV.Server.EntityFrameworkCore/Migrations/${file}`,
  sha256: crypto.createHash('sha256').update(lf(fs.readFileSync(path.join(migrationDirectory, file), 'utf8'))).digest('hex'),
}));
const manifest = {
  format: 1, latestMigration: '20240826092427_TenantProfileContractsServicePackagesTrigger',
  modelTableCount,
  constraints: [...constraints.keys()], functions: [...functions.keys()], triggers: [...triggers.keys()],
  rawOperations: operations.length, transientMigrationOperations: transient,
  schemaSource: 'PostgreSQL catalog after replaying 67 original migrations; only two transitions for an undefined ContractSettings trigger omitted.',
  seedSource: 'Latest model HasData: 5 ConstraintTypes, 18 TicketSections, 6 TicketTypes; not a copy of production data.',
  historyTable: 'Not fabricated: this is a fresh SQL baseline, not an executed EF migration history.',
  sourceFiles,
};
fs.writeFileSync(path.join(base, 'source-manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
console.log(`Generated ${manifest.modelTableCount} tables, ${functions.size} functions, ${triggers.size} triggers, ${constraints.size} raw unique constraints.`);
