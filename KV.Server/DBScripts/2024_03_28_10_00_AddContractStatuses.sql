INSERT INTO public."ContractStatuses" ("Id", "TenantId", "Title", "Code")
SELECT gen_random_uuid(), c."Id", 'Черновик', 'draft' FROM (select tp."Id" FROM "TenantProfiles" tp
left outer join public."ContractStatuses" cs on cs."TenantId" = tp."Id" and cs."Code" = 'draft'
where cs."Id" is null) c
UNION ALL
SELECT gen_random_uuid(), c."Id", 'Отменен', 'cancel' FROM (select tp."Id" FROM "TenantProfiles" tp
left outer join public."ContractStatuses" cs on cs."TenantId" = tp."Id" and cs."Code" = 'cancel'
where cs."Id" is null) c
UNION ALL
SELECT gen_random_uuid(), c."Id", 'Завершен', 'complete' FROM (select tp."Id" FROM "TenantProfiles" tp
left outer join public."ContractStatuses" cs on cs."TenantId" = tp."Id" and cs."Code" = 'complete'
where cs."Id" is null) c
UNION ALL
SELECT gen_random_uuid(), c."Id", 'В ходе выполнения', 'in progress' FROM (select tp."Id" FROM "TenantProfiles" tp
left outer join public."ContractStatuses" cs on cs."TenantId" = tp."Id" and cs."Code" = 'in progress'
where cs."Id" is null) c;