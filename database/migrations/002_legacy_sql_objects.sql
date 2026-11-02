-- Last original definition of each PostgreSQL function. Bodies are unchanged apart from line endings.

-- upsert_constraint_monthly_stats_trigger_fn
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create or replace
function upsert_constraint_monthly_stats_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public."ContractConstraintsByMonths"
("ContractId",
	"ConstraintTypeId",
	"Year",
	"Month",
	"MaxCount",
	"Sum")
select
	c."Id" as "ContractId",
	new."ConstraintTypeId",
	date_part('year', new."CreationTime") as "Year",
	date_part('month', new."CreationTime") as "Month",
	coalesce(s."Max", 0) as "MaxCount",
	sum(thsp."Spent") as "Sum"
from
	"TicketHoursSpentHistories" as thsp
join "TicketHistory" h on
	h."Id" = thsp."TicketHistoryId"
join "Tickets" t on
	t."Id" = h."TicketId"
join "Contracts" c on
	c."Id" = t."ContractId"
left outer join "ContractSettings" s on
	s."ContractId" = c."Id"
	and s."ConstraintTypeId" = new."ConstraintTypeId"
where
	thsp."ConstraintTypeId" = new."ConstraintTypeId"
	and c."Id" = 
(
	select
		tc."ContractId"
	from
		"TicketHistory" as thc
	join "Tickets" tc on
		tc."Id" = thc."TicketId"
	where
		thc."Id" = new."TicketHistoryId") and date_part('year', thsp."CreationTime") = date_part('year', new."CreationTime") and date_part('month', thsp."CreationTime") = date_part('month', new."CreationTime")
group by c."Id", s."Max"
	on
	conflict on
	constraint "uc_year_month_constrainttypeid"
do
update
set
	"MaxCount" = EXCLUDED."MaxCount",
	"Sum" = EXCLUDED."Sum";

return new;
end;

$$
language 'plpgsql';

-- upsert_monthly_stats_section_counters_trigger_fn
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create or replace
function upsert_monthly_stats_section_counters_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public."ContractTicketsByTicketSectionByMonths"
("ContractId",
	"TicketSectionId",
	"Year",
	"Month",
	"Count")
select
	new."ContractId",
	new."TicketSectionId",
	date_part('year', new."CreationTime") as "Year",
	date_part('month', new."CreationTime") as "Month",
	count(t."Id") as "Count"
from
	"Tickets" as t
where
	t."TicketSectionId" = new."TicketSectionId"
	and t."ContractId" = new."ContractId"
	and date_part('year', t."CreationTime") = date_part('year', new."CreationTime") and date_part('month', t."CreationTime") = date_part('month', new."CreationTime")
group by t."TicketSectionId", t."ContractId"
	on
	conflict on
	constraint "uc_year_month_ticketsectionid"
do
update
set
	"Count" = EXCLUDED."Count";

return new;
end;

$$
language 'plpgsql';

-- upsert_monthly_stats_types_counters_trigger_fn
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create or replace
function upsert_monthly_stats_types_counters_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public."ContractTicketsByTicketTypeByMonths"
("ContractId",
	"TicketTypeId",
	"Year",
	"Month",
	"Count")
select
	new."ContractId",
	new."TicketTypeId",
	date_part('year', new."CreationTime") as "Year",
	date_part('month', new."CreationTime") as "Month",
	count(t."Id") as "Count"
from
	"Tickets" as t
where
	t."TicketTypeId" = new."TicketTypeId"
	and t."ContractId" = new."ContractId"
	and date_part('year', t."CreationTime") = date_part('year', new."CreationTime") and date_part('month', t."CreationTime") = date_part('month', new."CreationTime")
group by t."TicketTypeId", t."ContractId"
	on
	conflict on
	constraint "uc_year_month_tickettypeid"
do
update
set
	"Count" = EXCLUDED."Count";

return new;
end;

$$
language 'plpgsql';

-- update_ticket_service_packages_trigger_fn
-- Source: KV.Server/.../Migrations/20240119103333_UpgradeToV8.cs (Up)
create or replace
function update_ticket_service_packages_trigger_fn()
  returns trigger as
$$
begin
	new."ServicePackages" = (
	select
		json_agg(res)
	from
		(
		select
			sp."Name",
			sp."Style",
			c."ContractStartDate",
			c."ContractFinishDate"
		from
			"Contracts" c
		join "ContractServicePackages" csp on
			csp."ContractId" = c."Id"
		join "ServicePackages" sp on
			sp."Id" = csp."ServicePackageId"
		where
			c."Id" = new."ContractId"
			and c."IsDeleted" = false
			and c."IsDraft" = false) as res);
return new;
end;

$$
language 'plpgsql';

-- update_contract_service_packages_trigger_fn
-- Source: KV.Server/.../Migrations/20240826092427_TenantProfileContractsServicePackagesTrigger.cs (Up)
CREATE OR REPLACE FUNCTION public.update_contract_service_packages_trigger_fn()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
begin
	update "Contracts" 
	set "ServicePackagesData" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp."Name",
			sp."Style",
			csp."ContractStartDate",
			csp."ContractFinishDate"
		from
			"Contracts" c
		join "ContractServicePackages" csp  on
			csp."ContractId" = c."Id"
		join "ServicePackages" sp on
			sp."Id" = csp."ServicePackageId"
		where
			c."Id" = new."ContractId"
			and c."IsDeleted" = false
			and c."IsDraft" = false) as res)
		where "Id" = new."ContractId";
	
	update "TenantProfiles"  
	set "ServicePackagesData" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp."Name",
			sp."Style",
			csp."ContractStartDate",
			csp."ContractFinishDate"
		from
			"Contracts" c
		join "ContractServicePackages" csp on
			csp."ContractId" = c."Id"
		join "ServicePackages" sp on
			sp."Id" = csp."ServicePackageId"
		where
			c."TenantId" = (select "TenantId" from "Contracts" where "Id" = new."ContractId")
			and c."IsDeleted" = false
			and c."IsDraft" = false) as res);
return new;
end;

$function$
;

-- update_contract_service_packages_trigger_delete_fn
-- Source: KV.Server/.../Migrations/20240826092427_TenantProfileContractsServicePackagesTrigger.cs (Up)
CREATE OR REPLACE FUNCTION public.update_contract_service_packages_trigger_delete_fn()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
begin
	update "Contracts" 
	set "ServicePackagesData" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp."Name",
			sp."Style",
			csp."ContractStartDate",
			csp."ContractFinishDate"
		from
			"Contracts" c
		join "ContractServicePackages" csp on
			csp."ContractId" = c."Id"
		join "ServicePackages" sp on
			sp."Id" = csp."ServicePackageId"
		where
			c."Id" = old."ContractId"
			and c."IsDeleted" = false
			and c."IsDraft" = false) as res)
		where "Id" = old."ContractId";
	
	update "TenantProfiles"  
	set "ServicePackagesData" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp."Name",
			sp."Style",
			csp."ContractStartDate",
			csp."ContractFinishDate"
		from
			"Contracts" c
		join "ContractServicePackages" csp on
			csp."ContractId" = c."Id"
		join "ServicePackages" sp on
			sp."Id" = csp."ServicePackageId"
		where
			c."TenantId" = (select "TenantId" from "Contracts" where "Id" = old."ContractId")
			and c."IsDeleted" = false
			and c."IsDraft" = false) as res);
return old;
end;

$function$
;

-- Final active triggers, after applying original CREATE/DROP operations in migration order.

-- TicketHoursSpentHistories.upsert_constraint_monthly_stats_trigger
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create trigger upsert_constraint_monthly_stats_trigger
  after
insert or update or delete
	on
	"TicketHoursSpentHistories"
  for each row
  execute procedure upsert_constraint_monthly_stats_trigger_fn();

-- Tickets.upsert_sections_monthly_stats_trigger
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create trigger upsert_sections_monthly_stats_trigger
  after
insert or update or delete
	on
	"Tickets"
  for each row
  execute procedure upsert_monthly_stats_section_counters_trigger_fn();

-- Tickets.upsert_types_monthly_stats_trigger
-- Source: KV.Server/.../Migrations/20231225072529_Updated_ConstraintsTriggers.cs (Up)
create trigger upsert_types_monthly_stats_trigger
  after
insert or update or delete
	on
	"Tickets"
  for each row
  execute procedure upsert_monthly_stats_types_counters_trigger_fn();

-- Tickets.update_ticket_service_packages_trigger
-- Source: KV.Server/.../Migrations/20240119103333_UpgradeToV8.cs (Up)
create trigger update_ticket_service_packages_trigger
  before
insert or update
	on
	"Tickets"
  for each row
  execute procedure 
 update_ticket_service_packages_trigger_fn();

-- ContractServicePackages.update_contract_service_packages_trigger
-- Source: KV.Server/.../Migrations/20240826092427_TenantProfileContractsServicePackagesTrigger.cs (Up)
create trigger update_contract_service_packages_trigger
  after
insert or update
	on
	"ContractServicePackages"
  for each row
  execute procedure 
 update_contract_service_packages_trigger_fn();

-- ContractServicePackages.delete_contract_service_packages_trigger
-- Source: KV.Server/.../Migrations/20240826092427_TenantProfileContractsServicePackagesTrigger.cs (Up)
create trigger delete_contract_service_packages_trigger
  after
delete
	on
	"ContractServicePackages"
  for each row
  execute procedure 
 update_contract_service_packages_trigger_delete_fn();
