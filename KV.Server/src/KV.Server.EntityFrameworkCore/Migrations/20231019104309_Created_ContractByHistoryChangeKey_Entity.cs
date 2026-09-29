using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedContractByHistoryChangeKeyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractTicketsByTicketTypeByMonths",
                table: "ContractTicketsByTicketTypeByMonths");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractTicketsByTicketSectionByMonths",
                table: "ContractTicketsByTicketSectionByMonths");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractConstraintsByMonths",
                table: "ContractConstraintsByMonths");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ContractTicketsByTicketTypeByMonths",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ContractTicketsByTicketSectionByMonths",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ContractConstraintsByMonths",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractTicketsByTicketTypeByMonths",
                table: "ContractTicketsByTicketTypeByMonths",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractTicketsByTicketSectionByMonths",
                table: "ContractTicketsByTicketSectionByMonths",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractConstraintsByMonths",
                table: "ContractConstraintsByMonths",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTicketsByTicketTypeByMonths_ContractId",
                table: "ContractTicketsByTicketTypeByMonths",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTicketsByTicketSectionByMonths_ContractId",
                table: "ContractTicketsByTicketSectionByMonths",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractConstraintsByMonths_ContractId",
                table: "ContractConstraintsByMonths",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractTicketsByTicketTypeByMonths",
                table: "ContractTicketsByTicketTypeByMonths");

            migrationBuilder.DropIndex(
                name: "IX_ContractTicketsByTicketTypeByMonths_ContractId",
                table: "ContractTicketsByTicketTypeByMonths");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractTicketsByTicketSectionByMonths",
                table: "ContractTicketsByTicketSectionByMonths");

            migrationBuilder.DropIndex(
                name: "IX_ContractTicketsByTicketSectionByMonths_ContractId",
                table: "ContractTicketsByTicketSectionByMonths");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractConstraintsByMonths",
                table: "ContractConstraintsByMonths");

            migrationBuilder.DropIndex(
                name: "IX_ContractConstraintsByMonths_ContractId",
                table: "ContractConstraintsByMonths");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContractTicketsByTicketTypeByMonths");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContractTicketsByTicketSectionByMonths");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContractConstraintsByMonths");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractTicketsByTicketTypeByMonths",
                table: "ContractTicketsByTicketTypeByMonths",
                columns: new[] { "ContractId", "TicketTypeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractTicketsByTicketSectionByMonths",
                table: "ContractTicketsByTicketSectionByMonths",
                columns: new[] { "ContractId", "TicketSectionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractConstraintsByMonths",
                table: "ContractConstraintsByMonths",
                columns: new[] { "ContractId", "ConstraintTypeId" });
        }
    }
}
