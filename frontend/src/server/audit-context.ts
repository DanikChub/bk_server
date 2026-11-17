import { AsyncLocalStorage } from 'node:async_hooks';
export type AuditContext={userId:string;tenantId:string|null;method:string;path:string;requestId:string};
export const auditContext=new AsyncLocalStorage<AuditContext>();
