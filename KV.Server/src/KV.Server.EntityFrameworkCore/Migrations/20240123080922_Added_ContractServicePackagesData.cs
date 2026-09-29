using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class Added_ContractServicePackagesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicePackagesData",
                table: "Contracts",
                type: "text",
                nullable: true);
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_contract_service_packages_trigger on
	""Contracts"";");
            migrationBuilder.Sql(@"drop function IF EXISTS  update_contract_service_packages_trigger_fn on
	""Contracts"";");

            migrationBuilder.DropColumn(
                name: "ServicePackagesData",
                table: "Contracts");

        }
    }
}
