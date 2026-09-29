using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KV.Server.Migrations
{
    public partial class Created_ContratStatus_Entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_TenantProfiles_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Contracts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_StatusId",
                table: "Contracts",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts",
                column: "StatusId",
                principalTable: "ContractStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ContractStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_StatusId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Contracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_TenantProfiles_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "TenantProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
