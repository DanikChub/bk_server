update public."EmployeeProfiles"
set "Email" = public."AbpUsers"."Email"
from public."AbpUsers"
where public."EmployeeProfiles"."IdentityUserId" = public."AbpUsers"."Id";

update public."CustomerUserProfiles"
set "Email" = public."AbpUsers"."Email"
from public."AbpUsers"
where public."CustomerUserProfiles"."IdentityUserId" = public."AbpUsers"."Id";

update public."AbpUsers"
set 
    "Email" = concat(public."AbpUsers"."UserName", '@kv.system'),
    "NormalizedEmail" = concat(upper(public."AbpUsers"."UserName"), '@KV.SYSTEM');

commit;