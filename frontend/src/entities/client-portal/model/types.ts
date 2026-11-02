export type PortalProfile = {
  id: string; name: string; position: string; phone: string; email: string; birthDate: string; avatarUrl?: string;
  organization: { name: string; address: string; inn: string; kpp: string; active: boolean };
  colleagues: { id: string; name: string }[];
};
export type PortalAttachment = { id: string; name: string; size: number; type: string };
export type PortalMessage = { id: string; author: string; text: string; createdAt: string; attachments: PortalAttachment[] };
export type PortalTicket = {
  id: string; subject: string; description: string; status: "Черновик" | "Новая" | "В работе" | "Требует уточнения" | "Закрыта";
  section: string; createdAt: string; specialist: string; favorite: boolean; rating: number | null;
  attachments: PortalAttachment[]; messages: PortalMessage[];
};
export type PortalContract = {
  id: string; number: string; package: string; startsAt: string; endsAt: string; progress: number; active: boolean;
  months: { month: string; limits: { title: string; total: number; used: number }[] }[];
};
export type PortalEvent = { id: string; date: string; title: string; completed: boolean; important: boolean };
export type PortalContent = {
  news: { id: string; title: string; body: string }[];
  articles: { id: string; title: string; body: string; icon: string }[];
};
export type PortalSnapshot = { profile: PortalProfile; tickets: PortalTicket[]; contracts: PortalContract[]; events: PortalEvent[]; content: PortalContent };
export type SaveQuestion = { id?: string; subject: string; description: string; section: string; draft: boolean; files: File[]; existingAttachments: PortalAttachment[] };

// UI contract. A Nest HTTP adapter can implement this without changing the views.
// The server must derive identity, organization and permissions from its session.
export interface PortalRepository {
  load(signal?: AbortSignal): Promise<PortalSnapshot>;
  saveQuestion(input: SaveQuestion): Promise<PortalTicket>;
  reply(ticketId: string, text: string, files: File[]): Promise<PortalTicket>;
  setFavorite(ticketId: string, favorite: boolean): Promise<PortalTicket>;
  rate(ticketId: string, rating: number): Promise<PortalTicket>;
  updateProfile(input: Pick<PortalProfile, "position" | "phone" | "email" | "birthDate">): Promise<PortalProfile>;
  updateAvatar(file: File): Promise<PortalProfile>;
  setEventCompleted(id: string, completed: boolean): Promise<PortalEvent>;
  requestRenewal(contractId: string, comment: string, files: File[]): Promise<void>;
  downloadAttachment(id: string): Promise<Blob>;
}
