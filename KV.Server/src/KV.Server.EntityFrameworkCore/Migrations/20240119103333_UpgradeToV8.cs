using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeToV8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicePackages",
                table: "Tickets",
                type: "text",
                nullable: true);


            migrationBuilder.Sql(@"create or replace
function update_ticket_service_packages_trigger_fn()
  returns trigger as
$$
begin
	new.""ServicePackages"" = (
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
			c.""Id"" = new.""ContractId""
			and c.""IsDeleted"" = false
			and c.""IsDraft"" = false) as res);
return new;
end;

$$
language 'plpgsql';");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_ticket_service_packages_trigger on
	""Tickets"";");
            migrationBuilder.Sql(@"create trigger update_ticket_service_packages_trigger
  before
insert or update
	on
	""Tickets""
  for each row
  execute procedure 
 update_ticket_service_packages_trigger_fn();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger IF EXISTS  update_ticket_service_packages_trigger on
	""Tickets"";");
            migrationBuilder.Sql(@"drop function IF EXISTS  update_ticket_service_packages_trigger_fn on
	""Tickets"";");

            migrationBuilder.DropColumn(
                name: "ServicePackages",
                table: "Tickets");

        }
    }
}
