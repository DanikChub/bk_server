

UPDATE public."AbpUserRoles" AS ur
SET "RoleId" = '3a0bad69-8956-14e3-3941-c9b152fb0963'
WHERE "RoleId" IN ('3a0bad6a-2fe4-e1d0-9529-dfb8a93a5e7b', '3a0bad6a-853b-4d0a-37cb-1094bafd2b6a', '3a0bef3c-ba97-c24d-8905-7dc8ac080e4e')
AND NOT EXISTS (
    SELECT 1
    FROM public."AbpUserRoles"
    WHERE "RoleId" = '3a0bad69-8956-14e3-3941-c9b152fb0963'
);

UPDATE public."AbpRoles"
SET "Name" = 'Администратор', 
    "NormalizedName" = 'АДМИНИСТРАТОР'
WHERE "Id" IN ('3a0bef39-4a1d-b967-5963-5a2be904733a');

DELETE FROM public."AbpRoles"
WHERE "Id"  = '3a0bad6a-853b-4d0a-37cb-1094bafd2b6a' OR "Id" = '3a0bef3c-ba97-c24d-8905-7dc8ac080e4e';

DELETE FROM public."AbpRoles"
WHERE "Id"  = '3a0bad6a-2fe4-e1d0-9529-dfb8a93a5e7b';
