import { portalFixture } from "../model/fixtures";
import type { PortalAttachment, PortalRepository, PortalSnapshot, PortalTicket, SaveQuestion } from "../model/types";

const storageKey = "consulting:client-portal:v1:demo-client-graf";
const maxBytes = 50 * 1024 * 1024;
const files = new Map<string, File>();
let memory: PortalSnapshot | undefined;
function snapshot(): PortalSnapshot {
  if (memory) return structuredClone(memory);
  try {
    const value: unknown = JSON.parse(localStorage.getItem(storageKey) ?? "null");
    if (isSnapshot(value)) memory = value;
  } catch { /* The demo also works with browser storage disabled. */ }
  memory ??= structuredClone(portalFixture);
  return structuredClone(memory);
}
function isSnapshot(value: unknown): value is PortalSnapshot {
  if (!value || typeof value !== "object") return false;
  const x = value as PortalSnapshot;
  const textFields = (item: unknown, fields: string[]) => Boolean(item && typeof item === "object" && fields.every(field => typeof (item as Record<string, unknown>)[field] === "string"));
  const attachmentList = (items: PortalAttachment[]) => Array.isArray(items) && items.every(item => textFields(item, ["id", "name", "type"]) && Number.isFinite(item.size) && item.size >= 0);
  return x.profile?.id === portalFixture.profile.id && textFields(x.profile, ["name", "position", "phone", "email", "birthDate"])
    && textFields(x.profile.organization, ["name", "address", "inn", "kpp"]) && typeof x.profile.organization.active === "boolean"
    && (x.profile.avatarUrl === undefined || typeof x.profile.avatarUrl === "string" && /^data:image\/(png|jpeg|webp);base64,/.test(x.profile.avatarUrl))
    && Array.isArray(x.profile.colleagues) && x.profile.colleagues.every(person => textFields(person, ["id", "name"]))
    && Array.isArray(x.tickets) && x.tickets.every(t => textFields(t, ["id", "subject", "description", "section", "createdAt", "specialist"])
      && ["Черновик", "Новая", "В работе", "Требует уточнения", "Закрыта"].includes(t.status) && Number.isFinite(Date.parse(t.createdAt))
      && typeof t.favorite === "boolean" && (t.rating === null || Number.isInteger(t.rating) && t.rating >= 1 && t.rating <= 5)
      && attachmentList(t.attachments) && Array.isArray(t.messages) && t.messages.every(m => textFields(m, ["id", "author", "text", "createdAt"]) && attachmentList(m.attachments)))
    && Array.isArray(x.contracts) && x.contracts.every(c => textFields(c, ["id", "number", "package", "startsAt", "endsAt"]) && Number.isFinite(c.progress) && typeof c.active === "boolean"
      && Array.isArray(c.months) && c.months.every(month => textFields(month, ["month"]) && Array.isArray(month.limits) && month.limits.every(limit => textFields(limit, ["title"]) && Number.isFinite(limit.total) && Number.isFinite(limit.used))))
    && Array.isArray(x.events) && x.events.every(e => textFields(e, ["id", "date", "title"]) && typeof e.completed === "boolean" && typeof e.important === "boolean")
    && Array.isArray(x.content?.news) && x.content.news.every(n => textFields(n, ["id", "title", "body"]))
    && Array.isArray(x.content?.articles) && x.content.articles.every(a => textFields(a, ["id", "title", "body", "icon"]));
}
function persist(next: PortalSnapshot) {
  memory = structuredClone(next);
  try { localStorage.setItem(storageKey, JSON.stringify(next)); } catch { /* In-memory fallback. */ }
}
export function validateQuestion(input: Pick<SaveQuestion, "subject" | "description" | "draft" | "files" | "existingAttachments">) {
  if (!input.subject.trim()) throw new Error("Укажите тему заявки.");
  if (input.subject.trim().length > 250) throw new Error("Тема должна содержать не более 250 символов.");
  if (!input.draft && !input.description.trim()) throw new Error("Опишите ваш вопрос.");
  if (input.files.reduce((sum, f) => sum + f.size, 0) + input.existingAttachments.reduce((sum, f) => sum + f.size, 0) > maxBytes) throw new Error("Общий размер вложений не должен превышать 50 МБ.");
}
function attachments(selected: File[]): PortalAttachment[] {
  return selected.map(file => { const id = crypto.randomUUID(); files.set(id, file); return { id, name: file.name, size: file.size, type: file.type }; });
}
function changeTicket(id: string, update: (ticket: PortalTicket) => void): PortalTicket {
  const next = snapshot(); const ticket = next.tickets.find(t => t.id === id);
  if (!ticket) throw new Error("Заявка не найдена или недоступна.");
  update(ticket); persist(next); return structuredClone(ticket);
}
export const demoPortalRepository: PortalRepository = {
  async load(signal) { if (signal?.aborted) throw new DOMException("Aborted", "AbortError"); return snapshot(); },
  async saveQuestion(input) {
    validateQuestion(input); const next = snapshot();
    if (!input.draft && !next.profile.organization.active) throw new Error("Для отправки вопроса необходимо продлить договор.");
    const old = input.id ? next.tickets.find(t => t.id === input.id) : undefined;
    if (input.id && (!old || old.status !== "Черновик")) throw new Error("Изменять можно только собственный черновик.");
    const ticket: PortalTicket = { id: old?.id ?? crypto.randomUUID(), subject: input.subject.trim(), description: input.description.trim(), section: input.section, status: input.draft ? "Черновик" : "Новая", createdAt: old?.createdAt ?? new Date().toISOString(), specialist: "Не назначен", favorite: old?.favorite ?? false, rating: null, attachments: [...input.existingAttachments, ...attachments(input.files)], messages: [] };
    next.tickets = [ticket, ...next.tickets.filter(t => t.id !== ticket.id)]; persist(next); return ticket;
  },
  async reply(id, text, selected) {
    validateQuestion({ subject: "Ответ", description: text, draft: false, files: selected, existingAttachments: [] });
    return changeTicket(id, ticket => {
      if (ticket.status === "Черновик" || ticket.status === "Закрыта") throw new Error("Отправка сообщений для этой заявки недоступна.");
      ticket.messages.push({ id: crypto.randomUUID(), author: snapshot().profile.name, text: text.trim(), createdAt: new Date().toISOString(), attachments: attachments(selected) });
      if (ticket.status === "Требует уточнения") ticket.status = "В работе";
    });
  },
  async setFavorite(id, favorite) { return changeTicket(id, ticket => { ticket.favorite = favorite; }); },
  async rate(id, rating) { return changeTicket(id, ticket => { if (ticket.status !== "Закрыта" || !Number.isInteger(rating) || rating < 1 || rating > 5) throw new Error("Оценить можно только закрытую заявку: от 1 до 5."); ticket.rating = rating; }); },
  async updateProfile(input) {
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(input.email) || !input.phone.trim()) throw new Error("Укажите корректные email и телефон.");
    const next = snapshot(); next.profile = { ...next.profile, ...input }; persist(next); return next.profile;
  },
  async setEventCompleted(id, completed) { const next = snapshot(); const event = next.events.find(e => e.id === id); if (!event) throw new Error("Событие не найдено."); event.completed = completed; persist(next); return event; },
  async updateAvatar(file) {
    if (!["image/png", "image/jpeg", "image/webp"].includes(file.type) || file.size > 5 * 1024 * 1024) throw new Error("Выберите PNG, JPEG или WebP размером до 5 МБ.");
    const avatarUrl = await new Promise<string>((resolve, reject) => { const reader = new FileReader(); reader.onload = () => resolve(String(reader.result)); reader.onerror = () => reject(new Error("Не удалось прочитать изображение.")); reader.readAsDataURL(file); });
    const next = snapshot(); next.profile.avatarUrl = avatarUrl; persist(next); return next.profile;
  },
  async requestRenewal(id, comment, selected) {
    const next = snapshot(); const contract = next.contracts.find(c => c.id === id); if (!contract) throw new Error("Договор не найден.");
    validateQuestion({ subject: "Продление договора", description: comment, draft: false, files: selected, existingAttachments: [] });
    // Local request is visible in the list; no message is sent to a real recipient.
    next.tickets.unshift({ id: crypto.randomUUID(), subject: `Продление договора № ${contract.number}`, description: comment.trim(), status: "Новая", section: "Договоры", createdAt: new Date().toISOString(), specialist: "Менеджер", favorite: false, rating: null, attachments: attachments(selected), messages: [] }); persist(next);
  },
  async downloadAttachment(id) { const file = files.get(id); if (!file) throw new Error("В демоверсии содержимое вложений доступно до перезагрузки страницы. Прикрепите файл повторно."); return file; },
};
