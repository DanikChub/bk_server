using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedConstraintsTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE public.""ContractConstraintsByMonths"" 
ADD CONSTRAINT uc_year_month_constrainttypeid 
UNIQUE (""ContractId"",""ConstraintTypeId"",""Year"",""Month"");");
            migrationBuilder.Sql(@"ALTER TABLE public.""ContractTicketsByTicketSectionByMonths"" 
ADD CONSTRAINT uc_year_month_ticketsectionid 
UNIQUE (""ContractId"",""TicketSectionId"",""Year"",""Month"");");
            migrationBuilder.Sql(@"ALTER TABLE public.""ContractTicketsByTicketTypeByMonths"" 
ADD CONSTRAINT uc_year_month_tickettypeid 
UNIQUE (""ContractId"",""TicketTypeId"",""Year"",""Month"");");

            migrationBuilder.Sql(@"create or replace
function upsert_constraint_monthly_stats_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public.""ContractConstraintsByMonths""
(""ContractId"",
	""ConstraintTypeId"",
	""Year"",
	""Month"",
	""MaxCount"",
	""Sum"")
select
	c.""Id"" as ""ContractId"",
	new.""ConstraintTypeId"",
	date_part('year', new.""CreationTime"") as ""Year"",
	date_part('month', new.""CreationTime"") as ""Month"",
	coalesce(s.""Max"", 0) as ""MaxCount"",
	sum(thsp.""Spent"") as ""Sum""
from
	""TicketHoursSpentHistories"" as thsp
join ""TicketHistory"" h on
	h.""Id"" = thsp.""TicketHistoryId""
join ""Tickets"" t on
	t.""Id"" = h.""TicketId""
join ""Contracts"" c on
	c.""Id"" = t.""ContractId""
left outer join ""ContractSettings"" s on
	s.""ContractId"" = c.""Id""
	and s.""ConstraintTypeId"" = new.""ConstraintTypeId""
where
	thsp.""ConstraintTypeId"" = new.""ConstraintTypeId""
	and c.""Id"" = 
(
	select
		tc.""ContractId""
	from
		""TicketHistory"" as thc
	join ""Tickets"" tc on
		tc.""Id"" = thc.""TicketId""
	where
		thc.""Id"" = new.""TicketHistoryId"") and date_part('year', thsp.""CreationTime"") = date_part('year', new.""CreationTime"") and date_part('month', thsp.""CreationTime"") = date_part('month', new.""CreationTime"")
group by c.""Id"", s.""Max""
	on
	conflict on
	constraint ""uc_year_month_constrainttypeid""
do
update
set
	""MaxCount"" = EXCLUDED.""MaxCount"",
	""Sum"" = EXCLUDED.""Sum"";

return new;
end;

$$
language 'plpgsql';");

			migrationBuilder.Sql(@"create or replace
function upsert_monthly_stats_section_counters_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public.""ContractTicketsByTicketSectionByMonths""
(""ContractId"",
	""TicketSectionId"",
	""Year"",
	""Month"",
	""Count"")
select
	new.""ContractId"",
	new.""TicketSectionId"",
	date_part('year', new.""CreationTime"") as ""Year"",
	date_part('month', new.""CreationTime"") as ""Month"",
	count(t.""Id"") as ""Count""
from
	""Tickets"" as t
where
	t.""TicketSectionId"" = new.""TicketSectionId""
	and t.""ContractId"" = new.""ContractId""
	and date_part('year', t.""CreationTime"") = date_part('year', new.""CreationTime"") and date_part('month', t.""CreationTime"") = date_part('month', new.""CreationTime"")
group by t.""TicketSectionId"", t.""ContractId""
	on
	conflict on
	constraint ""uc_year_month_ticketsectionid""
do
update
set
	""Count"" = EXCLUDED.""Count"";

return new;
end;

$$
language 'plpgsql';");
            migrationBuilder.Sql(@"create or replace
function upsert_monthly_stats_types_counters_trigger_fn()
  returns trigger as
$$
begin
 insert
	into
	public.""ContractTicketsByTicketTypeByMonths""
(""ContractId"",
	""TicketTypeId"",
	""Year"",
	""Month"",
	""Count"")
select
	new.""ContractId"",
	new.""TicketTypeId"",
	date_part('year', new.""CreationTime"") as ""Year"",
	date_part('month', new.""CreationTime"") as ""Month"",
	count(t.""Id"") as ""Count""
from
	""Tickets"" as t
where
	t.""TicketTypeId"" = new.""TicketTypeId""
	and t.""ContractId"" = new.""ContractId""
	and date_part('year', t.""CreationTime"") = date_part('year', new.""CreationTime"") and date_part('month', t.""CreationTime"") = date_part('month', new.""CreationTime"")
group by t.""TicketTypeId"", t.""ContractId""
	on
	conflict on
	constraint ""uc_year_month_tickettypeid""
do
update
set
	""Count"" = EXCLUDED.""Count"";

return new;
end;

$$
language 'plpgsql';");


            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_constraint_monthly_stats_trigger on
	""TicketHoursSpentHistories"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_sections_monthly_stats_trigger on
	""Tickets"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_types_monthly_stats_trigger on
	""Tickets"";");

            migrationBuilder.Sql(@"create trigger upsert_constraint_monthly_stats_trigger
  after
insert or update or delete
	on
	""TicketHoursSpentHistories""
  for each row
  execute procedure upsert_constraint_monthly_stats_trigger_fn();");
			migrationBuilder.Sql(@"create trigger upsert_sections_monthly_stats_trigger
  after
insert or update or delete
	on
	""Tickets""
  for each row
  execute procedure upsert_monthly_stats_section_counters_trigger_fn();");
            migrationBuilder.Sql(@"create trigger upsert_types_monthly_stats_trigger
  after
insert or update or delete
	on
	""Tickets""
  for each row
  execute procedure upsert_monthly_stats_types_counters_trigger_fn();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_constraint_monthly_stats_trigger on
	""TicketHoursSpentHistories"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_sections_monthly_stats_trigger on
	""Tickets"";");
            migrationBuilder.Sql(@"drop trigger IF EXISTS  upsert_types_monthly_stats_trigger on
	""Tickets"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS public.upsert_constraint_monthly_stats_trigger_fn();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS public.upsert_monthly_stats_section_counters_trigger_fn();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS public.upsert_monthly_stats_types_counters_trigger_fn();");
            migrationBuilder.DropUniqueConstraint("uc_year_month_constrainttypeid",
                "ContractConstraintsByMonths", "public");
            migrationBuilder.DropUniqueConstraint("uc_year_month_ticketsectionid",
                "ContractTicketsByTicketSectionByMonths", "public");
            migrationBuilder.DropUniqueConstraint("uc_year_month_tickettypeid",
                "ContractTicketsByTicketTypeByMonths", "public");
        }
    }
}
