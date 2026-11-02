-- Run only against a freshly initialized disposable database. All fixtures roll back.
BEGIN;
SET LOCAL search_path = public;
SET LOCAL plpgsql.check_asserts = on;

DO $legacy_behavior$
DECLARE
    tenant_a uuid := 'f0000000-0000-0000-0000-000000000001';
    tenant_b uuid := 'f0000000-0000-0000-0000-000000000002';
    contract_a uuid := 'f0000000-0000-0000-0000-000000000003';
    contract_status uuid := 'f0000000-0000-0000-0000-000000000004';
    package_a uuid := 'f0000000-0000-0000-0000-000000000005';
    ticket_status uuid := 'f0000000-0000-0000-0000-000000000006';
    history_id uuid := 'f0000000-0000-0000-0000-000000000007';
    spent_id uuid := 'f0000000-0000-0000-0000-000000000008';
    setting_id uuid := 'f0000000-0000-0000-0000-000000000009';
    file_id uuid := 'f0000000-0000-0000-0000-00000000000a';
    constraint_type uuid := '5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65';
    ticket_type uuid := 'a736e1b2-3ad6-471f-971f-d2a27f86a416';
    ticket_section uuid := '1736e1b2-3ad6-471f-971f-d2a27f86a417';
    ticket_id bigint;
    rejected boolean;
    payload jsonb;
BEGIN
    IF EXISTS (SELECT 1 FROM "TenantProfiles") OR EXISTS (SELECT 1 FROM "Tickets")
       OR EXISTS (SELECT 1 FROM "AbpUsers") THEN
        RAISE EXCEPTION 'Behavior verification requires a disposable database without users, organizations or tickets';
    END IF;

    ASSERT (SELECT count(*) FROM pg_tables WHERE schemaname = 'public') = 69, 'Expected exactly 69 tables';
    ASSERT to_regclass('public."__EFMigrationsHistory"') IS NULL, 'No fabricated EF migration history';
    ASSERT (SELECT count(*) FROM pg_trigger WHERE NOT tgisinternal) = 6, 'Expected six original triggers';
    ASSERT (SELECT count(*) FROM "ConstraintTypes") = 5, 'Original constraint types';
    ASSERT (SELECT count(*) FROM "TicketTypes") = 6, 'Latest model includes the written ticket type';
    ASSERT (SELECT count(*) FROM "TicketSections") = 18, 'Original ticket sections';

    INSERT INTO "TenantProfiles" ("Id", "ShortName", "LongName", "CreationTime", "ExtraProperties")
    VALUES (tenant_a, 'Fixture A', 'Fixture organization A', '2026-11-01', '{}'),
           (tenant_b, 'Fixture B', 'Fixture organization B', '2026-11-01', '{}');
    ASSERT NOT (SELECT "IsDeleted" FROM "TenantProfiles" WHERE "Id" = tenant_a), 'Legacy soft-delete default';

    INSERT INTO "ContractStatuses" ("Id", "Code", "Title", "TenantId")
    VALUES (contract_status, 'test-active', 'Fixture status', tenant_a);
    INSERT INTO "Contracts" ("Id", "StatusId", "TenantId", "ContractStartDate", "ContractFinishDate", "CreationTime", "Name")
    VALUES (contract_a, contract_status, tenant_a, '2026-01-01', '2026-12-31', '2026-01-01', 'Fixture contract');
    INSERT INTO "ServicePackages" ("Id", "Name", "Style") VALUES (package_a, 'Fixture package', 'test');
    INSERT INTO "ContractServicePackages" ("ContractId", "ServicePackageId", "ContractStartDate", "ContractFinishDate")
    VALUES (contract_a, package_a, '2026-02-01', '2026-11-30');

    SELECT "ServicePackagesData"::jsonb INTO payload FROM "Contracts" WHERE "Id" = contract_a;
    ASSERT payload->0->>'Name' = 'Fixture package', 'Package trigger updates its contract';
    ASSERT (payload->0->>'ContractStartDate')::timestamp = '2026-02-01'::timestamp, 'Contract payload uses package dates';
    ASSERT (SELECT "ServicePackagesData"::jsonb->0->>'Name' FROM "TenantProfiles" WHERE "Id" = tenant_a) = 'Fixture package', 'Package trigger updates tenant cache';
    -- Known original defect, deliberately not repaired: the UPDATE has no outer WHERE.
    ASSERT (SELECT "ServicePackagesData"::jsonb->0->>'Name' FROM "TenantProfiles" WHERE "Id" = tenant_b) = 'Fixture package', 'Original cross-tenant cache update behavior must remain visible';

    rejected := false;
    BEGIN
        INSERT INTO "ContractServicePackages" ("ContractId", "ServicePackageId", "ContractStartDate", "ContractFinishDate")
        VALUES (contract_a, package_a, '2026-02-01', '2026-11-30');
    EXCEPTION WHEN unique_violation THEN rejected := true;
    END;
    ASSERT rejected, 'Composite package key must reject duplicates';

    INSERT INTO "TicketStatuses" ("Id", "Name", "DisplayName") VALUES (ticket_status, 'Fixture', 'Fixture');
    INSERT INTO "Tickets" ("TenantId", "ContractId", "TicketTypeId", "TicketSectionId", "TicketStatusId", "Subject", "Description", "CreationTime", "DueDate", "AttachmentsCount", "ExtraProperties")
    VALUES (tenant_a, contract_a, ticket_type, ticket_section, ticket_status, 'Fixture question', 'Fixture text', '2026-11-02 12:00:00', '2026-11-04', 0, '{}')
    RETURNING "Id" INTO ticket_id;
    ASSERT ticket_id > 0, 'Ticket identity sequence works';
    ASSERT (SELECT "Rating" FROM "Tickets" WHERE "Id" = ticket_id) = 0, 'Migration-only rating default preserved';
    SELECT "ServicePackages"::jsonb INTO payload FROM "Tickets" WHERE "Id" = ticket_id;
    ASSERT payload->0->>'Name' = 'Fixture package', 'Ticket package trigger works';
    ASSERT (payload->0->>'ContractStartDate')::timestamp = '2026-01-01'::timestamp, 'Original ticket payload uses contract dates';
    ASSERT (SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId" = contract_a AND "TicketTypeId" = ticket_type AND "Year" = 2026 AND "Month" = 11) = 1, 'Type monthly counter';
    ASSERT (SELECT "Count" FROM "ContractTicketsByTicketSectionByMonths" WHERE "ContractId" = contract_a AND "TicketSectionId" = ticket_section AND "Year" = 2026 AND "Month" = 11) = 1, 'Section monthly counter';

    INSERT INTO "ContractSettings" ("Id", "ContractId", "ConstraintTypeId", "Max", "CreationTime")
    VALUES (setting_id, contract_a, constraint_type, 30, '2026-11-01');
    INSERT INTO "TicketHistory" ("Id", "TicketId", "TicketStatusId", "CreationTime", "Description")
    VALUES (history_id, ticket_id, ticket_status, '2026-11-02T12:00:00Z', 'Fixture response');
    INSERT INTO "TicketHoursSpentHistories" ("Id", "TicketHistoryId", "ConstraintTypeId", "Spent", "CreationTime")
    VALUES (spent_id, history_id, constraint_type, 3, '2026-11-02');
    ASSERT (SELECT "MaxCount" FROM "ContractConstraintsByMonths" WHERE "ContractId" = contract_a AND "ConstraintTypeId" = constraint_type AND "Year" = 2026 AND "Month" = 11) = 30, 'Monthly limit comes from contract settings';
    ASSERT (SELECT "Sum" FROM "ContractConstraintsByMonths" WHERE "ContractId" = contract_a AND "ConstraintTypeId" = constraint_type AND "Year" = 2026 AND "Month" = 11) = 3, 'Spending trigger inserts monthly total';
    UPDATE "TicketHoursSpentHistories" SET "Spent" = 5 WHERE "Id" = spent_id;
    ASSERT (SELECT "Sum" FROM "ContractConstraintsByMonths" WHERE "ContractId" = contract_a AND "ConstraintTypeId" = constraint_type AND "Year" = 2026 AND "Month" = 11) = 5, 'Spending update recomputes total';
    ASSERT (SELECT count(*) FROM "ContractConstraintsByMonths" WHERE "ContractId" = contract_a AND "ConstraintTypeId" = constraint_type AND "Year" = 2026 AND "Month" = 11) = 1, 'ON CONFLICT uses the original named unique constraint';

    INSERT INTO "UploadedFiles" ("Id", "CreationTime", "SizeInBytes", "FileName") VALUES (file_id, '2026-11-02', 100, 'fixture.txt');
    INSERT INTO "TicketAttachments" ("TicketId", "UploadedFileId") VALUES (ticket_id, file_id);
    rejected := false;
    BEGIN
        INSERT INTO "TicketAttachments" ("TicketId", "UploadedFileId") VALUES (9223372036854775807, file_id);
    EXCEPTION WHEN foreign_key_violation THEN rejected := true;
    END;
    ASSERT rejected, 'Attachment FK must reject an unknown ticket';

    -- Original DELETE handlers use NEW instead of OLD; totals stay stale. Preserve,
    -- expose and document the behavior rather than silently changing the migration.
    DELETE FROM "TicketHoursSpentHistories" WHERE "Id" = spent_id;
    ASSERT (SELECT "Sum" FROM "ContractConstraintsByMonths" WHERE "ContractId" = contract_a AND "ConstraintTypeId" = constraint_type AND "Year" = 2026 AND "Month" = 11) = 5, 'Known original delete-counter behavior';
    DELETE FROM "Tickets" WHERE "Id" = ticket_id;
    ASSERT NOT EXISTS (SELECT 1 FROM "TicketHistory" WHERE "Id" = history_id), 'Ticket history cascades on physical ticket deletion';
    ASSERT NOT EXISTS (SELECT 1 FROM "TicketAttachments" WHERE "TicketId" = ticket_id), 'Attachment link cascades';
    ASSERT EXISTS (SELECT 1 FROM "UploadedFiles" WHERE "Id" = file_id), 'File metadata is not deleted with a link';
    ASSERT (SELECT "Count" FROM "ContractTicketsByTicketTypeByMonths" WHERE "ContractId" = contract_a AND "TicketTypeId" = ticket_type AND "Year" = 2026 AND "Month" = 11) = 1, 'Known original ticket delete-counter behavior';

    DELETE FROM "ContractServicePackages" WHERE "ContractId" = contract_a AND "ServicePackageId" = package_a;
    ASSERT (SELECT "ServicePackagesData" FROM "Contracts" WHERE "Id" = contract_a) IS NULL, 'Package DELETE trigger clears contract cache';
    ASSERT (SELECT "ServicePackagesData" FROM "TenantProfiles" WHERE "Id" = tenant_b) IS NULL, 'Original package DELETE updates all tenant caches';
    RAISE NOTICE 'All legacy schema and trigger behavior assertions passed (including documented original defects).';
END;
$legacy_behavior$;

ROLLBACK;
