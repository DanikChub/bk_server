-- Runtime correction of legacy counters. The replayable legacy baseline stays unchanged.
-- Recalculate both old and new groups; exclude soft-deleted tickets/history;
-- choose one contract-specific setting instead of multiplying spends by setting history.
BEGIN;
CREATE OR REPLACE FUNCTION next_refresh_contract_statistics(contract_id uuid) RETURNS void AS $$
BEGIN
  IF contract_id IS NULL OR NOT EXISTS(SELECT 1 FROM "Contracts" WHERE "Id"=contract_id) THEN RETURN; END IF;
  -- Compatible with the KEY SHARE lock acquired by ticket/settings foreign keys.
  -- FOR UPDATE here makes concurrent inserts upgrade each other's FK locks and deadlock.
  PERFORM 1 FROM "Contracts" WHERE "Id"=contract_id FOR NO KEY UPDATE;
  UPDATE "ContractTicketsByTicketTypeByMonths" SET "Count"=0 WHERE "ContractId"=contract_id;
  INSERT INTO "ContractTicketsByTicketTypeByMonths"("ContractId","TicketTypeId","Year","Month","Count")
    SELECT contract_id,t."TicketTypeId",extract(year FROM t."CreationTime"),extract(month FROM t."CreationTime"),count(*)
    FROM "Tickets" t WHERE t."ContractId"=contract_id AND NOT t."IsDeleted"
    GROUP BY t."TicketTypeId",extract(year FROM t."CreationTime"),extract(month FROM t."CreationTime")
    ON CONFLICT ON CONSTRAINT uc_year_month_tickettypeid DO UPDATE SET "Count"=excluded."Count";
  UPDATE "ContractTicketsByTicketSectionByMonths" SET "Count"=0 WHERE "ContractId"=contract_id;
  INSERT INTO "ContractTicketsByTicketSectionByMonths"("ContractId","TicketSectionId","Year","Month","Count")
    SELECT contract_id,t."TicketSectionId",extract(year FROM t."CreationTime"),extract(month FROM t."CreationTime"),count(*)
    FROM "Tickets" t WHERE t."ContractId"=contract_id AND NOT t."IsDeleted" AND t."TicketSectionId" IS NOT NULL
    GROUP BY t."TicketSectionId",extract(year FROM t."CreationTime"),extract(month FROM t."CreationTime")
    ON CONFLICT ON CONSTRAINT uc_year_month_ticketsectionid DO UPDATE SET "Count"=excluded."Count";
  UPDATE "ContractConstraintsByMonths" SET "Sum"=0 WHERE "ContractId"=contract_id;
  INSERT INTO "ContractConstraintsByMonths"("ContractId","ConstraintTypeId","Year","Month","Sum","MaxCount")
    SELECT contract_id,spent.kind,spent.stat_year,spent.stat_month,spent.total,coalesce(setting."Max",0)
    FROM (
      SELECT x."ConstraintTypeId" kind,extract(year FROM x."CreationTime")::integer stat_year,extract(month FROM x."CreationTime")::integer stat_month,sum(x."Spent") total
      FROM "TicketHoursSpentHistories" x JOIN "TicketHistory" h ON h."Id"=x."TicketHistoryId" JOIN "Tickets" t ON t."Id"=h."TicketId"
      WHERE t."ContractId"=contract_id AND NOT t."IsDeleted" AND NOT h."IsDeleted"
      GROUP BY x."ConstraintTypeId",extract(year FROM x."CreationTime"),extract(month FROM x."CreationTime")
    ) spent LEFT JOIN LATERAL (
      SELECT s."Max" FROM "ContractSettings" s WHERE s."ContractId"=contract_id AND s."ConstraintTypeId"=spent.kind
      ORDER BY s."CreationTime" DESC,s."Id" DESC LIMIT 1
    ) setting ON true
    ON CONFLICT ON CONSTRAINT uc_year_month_constrainttypeid DO UPDATE SET "Sum"=excluded."Sum",
      "MaxCount"=CASE WHEN excluded."Year"=extract(year FROM now()) AND excluded."Month"=extract(month FROM now()) THEN excluded."MaxCount" ELSE "ContractConstraintsByMonths"."MaxCount" END;
  INSERT INTO "ContractConstraintsByMonths"("ContractId","ConstraintTypeId","Year","Month","Sum","MaxCount")
    SELECT DISTINCT ON(s."ConstraintTypeId") contract_id,s."ConstraintTypeId",extract(year FROM now()),extract(month FROM now()),0,s."Max"
    FROM "ContractSettings" s WHERE s."ContractId"=contract_id ORDER BY s."ConstraintTypeId",s."CreationTime" DESC,s."Id" DESC
    ON CONFLICT ON CONSTRAINT uc_year_month_constrainttypeid DO UPDATE SET "MaxCount"=excluded."MaxCount";
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION upsert_monthly_stats_types_counters_trigger_fn() RETURNS trigger AS $$
BEGIN
  IF TG_OP='UPDATE' AND (OLD."ContractId",OLD."TicketTypeId",OLD."TicketSectionId",OLD."CreationTime",OLD."IsDeleted") IS NOT DISTINCT FROM
      (NEW."ContractId",NEW."TicketTypeId",NEW."TicketSectionId",NEW."CreationTime",NEW."IsDeleted") THEN RETURN NEW; END IF;
  IF TG_OP<>'INSERT' THEN PERFORM next_refresh_contract_statistics(OLD."ContractId"); END IF;
  IF TG_OP<>'DELETE' THEN PERFORM next_refresh_contract_statistics(NEW."ContractId"); RETURN NEW; END IF;
  RETURN OLD;
END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION upsert_monthly_stats_section_counters_trigger_fn() RETURNS trigger AS $$
BEGIN
  -- The type trigger refreshes both groups atomically. Keep the original trigger binding valid.
  IF TG_OP='DELETE' THEN RETURN OLD; END IF; RETURN NEW;
END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION upsert_constraint_monthly_stats_trigger_fn() RETURNS trigger AS $$
DECLARE previous_contract uuid; current_contract uuid;
BEGIN
  IF TG_OP<>'INSERT' THEN SELECT t."ContractId" INTO previous_contract FROM "TicketHistory" h JOIN "Tickets" t ON t."Id"=h."TicketId" WHERE h."Id"=OLD."TicketHistoryId"; END IF;
  IF TG_OP<>'DELETE' THEN SELECT t."ContractId" INTO current_contract FROM "TicketHistory" h JOIN "Tickets" t ON t."Id"=h."TicketId" WHERE h."Id"=NEW."TicketHistoryId"; END IF;
  PERFORM next_refresh_contract_statistics(previous_contract);
  IF current_contract IS DISTINCT FROM previous_contract THEN PERFORM next_refresh_contract_statistics(current_contract); END IF;
  IF TG_OP='DELETE' THEN RETURN OLD; END IF; RETURN NEW;
END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION next_statistics_related_trigger_fn() RETURNS trigger AS $$
DECLARE previous_contract uuid; current_contract uuid;
BEGIN
  IF TG_TABLE_NAME='ContractSettings' THEN
    IF TG_OP<>'INSERT' THEN previous_contract:=OLD."ContractId"; END IF;
    IF TG_OP<>'DELETE' THEN current_contract:=NEW."ContractId"; END IF;
  ELSE
    IF TG_OP='UPDATE' AND (OLD."TicketId",OLD."IsDeleted") IS NOT DISTINCT FROM (NEW."TicketId",NEW."IsDeleted") THEN RETURN NEW; END IF;
    IF TG_OP<>'INSERT' THEN SELECT "ContractId" INTO previous_contract FROM "Tickets" WHERE "Id"=OLD."TicketId"; END IF;
    IF TG_OP<>'DELETE' THEN SELECT "ContractId" INTO current_contract FROM "Tickets" WHERE "Id"=NEW."TicketId"; END IF;
  END IF;
  PERFORM next_refresh_contract_statistics(previous_contract);
  IF current_contract IS DISTINCT FROM previous_contract THEN PERFORM next_refresh_contract_statistics(current_contract); END IF;
  IF TG_OP='DELETE' THEN RETURN OLD; END IF; RETURN NEW;
END;
$$ LANGUAGE plpgsql;
DROP TRIGGER IF EXISTS next_settings_statistics ON "ContractSettings";
CREATE TRIGGER next_settings_statistics AFTER INSERT OR UPDATE OR DELETE ON "ContractSettings" FOR EACH ROW EXECUTE FUNCTION next_statistics_related_trigger_fn();
DROP TRIGGER IF EXISTS next_history_statistics ON "TicketHistory";
CREATE TRIGGER next_history_statistics AFTER UPDATE OR DELETE ON "TicketHistory" FOR EACH ROW EXECUTE FUNCTION next_statistics_related_trigger_fn();
COMMIT;
