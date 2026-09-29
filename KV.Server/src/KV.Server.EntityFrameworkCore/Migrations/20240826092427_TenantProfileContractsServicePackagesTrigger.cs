using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class TenantProfileContractsServicePackagesTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_contract_service_packages_trigger on
	""Contracts"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_contract_service_packages_trigger on
	""ContractServicePackages"";");
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.update_contract_service_packages_trigger_fn()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
begin
	update ""Contracts"" 
	set ""ServicePackagesData"" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp.""Name"",
			sp.""Style"",
			csp.""ContractStartDate"",
			csp.""ContractFinishDate""
		from
			""Contracts"" c
		join ""ContractServicePackages"" csp  on
			csp.""ContractId"" = c.""Id""
		join ""ServicePackages"" sp on
			sp.""Id"" = csp.""ServicePackageId""
		where
			c.""Id"" = new.""ContractId""
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res)
		where ""Id"" = new.""ContractId"";
	
	update ""TenantProfiles""  
	set ""ServicePackagesData"" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp.""Name"",
			sp.""Style"",
			csp.""ContractStartDate"",
			csp.""ContractFinishDate""
		from
			""Contracts"" c
		join ""ContractServicePackages"" csp on
			csp.""ContractId"" = c.""Id""
		join ""ServicePackages"" sp on
			sp.""Id"" = csp.""ServicePackageId""
		where
			c.""TenantId"" = (select ""TenantId"" from ""Contracts"" where ""Id"" = new.""ContractId"")
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res);
return new;
end;

$function$
;
");
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.update_contract_service_packages_trigger_delete_fn()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
begin
	update ""Contracts"" 
	set ""ServicePackagesData"" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp.""Name"",
			sp.""Style"",
			csp.""ContractStartDate"",
			csp.""ContractFinishDate""
		from
			""Contracts"" c
		join ""ContractServicePackages"" csp on
			csp.""ContractId"" = c.""Id""
		join ""ServicePackages"" sp on
			sp.""Id"" = csp.""ServicePackageId""
		where
			c.""Id"" = old.""ContractId""
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res)
		where ""Id"" = old.""ContractId"";
	
	update ""TenantProfiles""  
	set ""ServicePackagesData"" = (
	select
		json_agg(res)
	from
		(
		select distinct 
			sp.""Name"",
			sp.""Style"",
			csp.""ContractStartDate"",
			csp.""ContractFinishDate""
		from
			""Contracts"" c
		join ""ContractServicePackages"" csp on
			csp.""ContractId"" = c.""Id""
		join ""ServicePackages"" sp on
			sp.""Id"" = csp.""ServicePackageId""
		where
			c.""TenantId"" = (select ""TenantId"" from ""Contracts"" where ""Id"" = old.""ContractId"")
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res);
return old;
end;

$function$
;");
            migrationBuilder.Sql(@"create trigger update_contract_service_packages_trigger
  after
insert or update
	on
	""ContractServicePackages""
  for each row
  execute procedure 
 update_contract_service_packages_trigger_fn();");
            migrationBuilder.Sql(@"create trigger delete_contract_service_packages_trigger
  after
delete
	on
	""ContractServicePackages""
  for each row
  execute procedure 
 update_contract_service_packages_trigger_delete_fn();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_contract_service_packages_trigger on
	""Contracts"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  delete_contract_service_packages_trigger on
	""Contracts"";");

            migrationBuilder.Sql(@"create or replace
function update_contract_service_packages_trigger_fn()
  returns trigger as
$$
begin
	new.""ServicePackagesData"" = (
	select
		json_agg(res)
	from
		(
		select
			sp.""Name"",
			sp.""Style"",
			c.""ContractStartDate"",
			c.""ContractFinishDate""
		from
			""Contracts"" c
		join ""ContractServicePackages"" csp on
			csp.""ContractId"" = c.""Id""
		join ""ServicePackages"" sp on
			sp.""Id"" = csp.""ServicePackageId""
		where
			c.""Id"" = new.""Id""
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res);
return new;
end;

$$
language 'plpgsql';");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_contract_service_packages_trigger on
	""Contracts"";");
            migrationBuilder.Sql(@"create trigger update_contract_service_packages_trigger
  before
insert or update
	on
	""Contracts""
  for each row
  execute procedure 
 update_contract_service_packages_trigger_fn();");
        }
    }
}
