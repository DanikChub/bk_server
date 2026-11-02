import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { loadModel } from './helpers/load-model.mjs';
const { demoPortalRepository: repository, validateQuestion } = loadModel(fileURLToPath(new URL('../src/entities/client-portal/api/demo-repository.ts', import.meta.url)));

test('A draft can be reopened and submitted using the same identity without duplicate tickets', async () => {
  const input = { subject: '  Проверка черновика  ', description: '', draft: true, files: [], existingAttachments: [], section: 'Консультация' };
  const draft = await repository.saveQuestion(input);
  assert.equal(draft.status, 'Черновик');
  await assert.rejects(repository.saveQuestion({ ...input, id: draft.id, draft: false }), /Опишите/);
  const submitted = await repository.saveQuestion({ ...input, id: draft.id, description: 'Уточнение вопроса', draft: false });
  assert.equal(submitted.id, draft.id); assert.equal(submitted.status, 'Новая');
  assert.equal((await repository.load()).tickets.filter(t => t.id === draft.id).length, 1);
  await assert.rejects(repository.saveQuestion({ ...input, id: draft.id }), /только собственный черновик/);
});

test('File limits account for both retained and newly selected attachments', () => {
  const base = { subject: 'Вопрос', description: 'Текст', draft: false, files: [], existingAttachments: [] };
  assert.throws(() => validateQuestion({ ...base, subject: ' ' }), /тему/);
  assert.throws(() => validateQuestion({ ...base, subject: 'x'.repeat(251) }), /250/);
  const file = { size: 30 * 1024 * 1024 };
  assert.throws(() => validateQuestion({ ...base, files: [file], existingAttachments: [file] }), /50 МБ/);
  assert.doesNotThrow(() => validateQuestion({ ...base, files: [{ size: 50 * 1024 * 1024 }] }));
});

test('Closed tickets cannot accept replies; ratings require a closed ticket and integer value', async () => {
  await assert.rejects(repository.reply('165565', 'Ответ', []), /недоступна/);
  await assert.rejects(repository.rate('165593', 5), /закрытую/);
  await assert.rejects(repository.rate('165565', 6), /от 1 до 5/);
  const rated = await repository.rate('165565', 5); assert.equal(rated.rating, 5);
  const updated = await repository.reply('165591', 'Согласовано с сотрудником', []);
  assert.equal(updated.status, 'В работе'); assert.equal(updated.messages.at(-1).text, 'Согласовано с сотрудником');
});

test('Unavailable records fail instead of modifying another record; returned snapshots are detached', async () => {
  await assert.rejects(repository.setFavorite('missing', true), /не найдена/);
  await assert.rejects(repository.requestRenewal('missing', 'Продлить', []), /не найден/);
  const first = await repository.load(); first.profile.name = 'Changed';
  assert.notEqual((await repository.load()).profile.name, 'Changed');
  const controller = new AbortController(); controller.abort(); await assert.rejects(repository.load(controller.signal), { name: 'AbortError' });
});

test('Renewal requests stay in the demo and retain the selected contract number', async () => {
  const before = await repository.load();
  await repository.requestRenewal('c2', 'Прошу продлить договор.', []);
  const after = await repository.load(); assert.equal(after.tickets.length, before.tickets.length + 1);
  assert.equal(after.tickets[0].subject, 'Продление договора № 131/23');
});
