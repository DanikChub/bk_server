BEGIN;
CREATE TABLE IF NOT EXISTS "NextRecoveryTokens" (
  "TokenHash" text PRIMARY KEY,
  "UserId" uuid NOT NULL REFERENCES "AbpUsers"("Id") ON DELETE CASCADE,
  "Purpose" text NOT NULL CHECK ("Purpose" IN ('password-reset','email-mfa')),
  "SecurityStamp" text,
  "ExpiresAt" timestamptz NOT NULL,
  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  UNIQUE("UserId","Purpose")
);
CREATE INDEX IF NOT EXISTS "NextRecoveryTokens_ExpiresAt" ON "NextRecoveryTokens"("ExpiresAt");
COMMIT;
