-- Additive migration for the TypeScript runtime. Apply after init.sql, never rerun init.sql.
BEGIN;
CREATE TABLE IF NOT EXISTS public."NextSessions" (
  "TokenHash" text PRIMARY KEY,
  "UserId" uuid NOT NULL REFERENCES public."AbpUsers"("Id") ON DELETE CASCADE,
  "TenantId" uuid,
  "SecurityStamp" text,
  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  "ExpiresAt" timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS "IX_NextSessions_UserId" ON public."NextSessions"("UserId");
CREATE INDEX IF NOT EXISTS "IX_NextSessions_ExpiresAt" ON public."NextSessions"("ExpiresAt");
CREATE TABLE IF NOT EXISTS public."NextJobs" (
  "Id" uuid PRIMARY KEY,
  "Kind" text NOT NULL,
  "Payload" jsonb NOT NULL,
  "Attempts" integer NOT NULL DEFAULT 0,
  "AvailableAt" timestamptz NOT NULL DEFAULT now(),
  "LockedUntil" timestamptz,
  "CompletedAt" timestamptz,
  "LastError" text,
  "DedupeKey" text UNIQUE,
  "CreatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "IX_NextJobs_Pending" ON public."NextJobs"("AvailableAt") WHERE "CompletedAt" IS NULL;
CREATE TABLE IF NOT EXISTS public."NextLoginAttempts" (
  "Key" text PRIMARY KEY,
  "WindowStart" timestamptz NOT NULL,
  "Count" integer NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS public."NextAuditLog" (
  "Id" uuid PRIMARY KEY,
  "UserId" uuid NOT NULL,
  "TenantId" uuid,
  "Method" text NOT NULL,
  "Path" text NOT NULL,
  "RequestId" uuid NOT NULL,
  "CreatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "IX_NextAuditLog_UserId_CreatedAt" ON public."NextAuditLog"("UserId","CreatedAt");
CREATE TABLE IF NOT EXISTS public."NextCache" (
  "Key" text PRIMARY KEY,
  "Value" jsonb NOT NULL,
  "ExpiresAt" timestamptz NOT NULL
);
CREATE TABLE IF NOT EXISTS public."NextInboxMessages" (
  "MessageId" text PRIMARY KEY,
  "ReceivedAt" timestamptz NOT NULL DEFAULT now()
);
COMMIT;
