using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    public partial class Updated_Entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePackages_Contracts_ContractId",
                table: "ServicePackages");

            migrationBuilder.DropIndex(
                name: "IX_ServicePackages_ContractId",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "ServicePackages");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "UploadedFiles",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "TicketStatuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "TicketStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Tickets",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "TenantProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "CommentNotes",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractEmail",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractOwner",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractPhoneNumber",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "INNNumber",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TenantProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KPPNumber",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegionId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteUrl",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirthDay",
                table: "CustomerUserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobPost",
                table: "CustomerUserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerUserProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ContractStatuses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_RegionId",
                table: "TenantProfiles",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_Regions_RegionId",
                table: "TenantProfiles",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_Regions_RegionId",
                table: "TenantProfiles");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_TenantProfiles_RegionId",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "TicketStatuses");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "TicketStatuses");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CommentNotes",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ContractEmail",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ContractOwner",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ContractPhoneNumber",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "INNNumber",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "KPPNumber",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "SiteUrl",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirthDay",
                table: "CustomerUserProfiles");

            migrationBuilder.DropColumn(
                name: "JobPost",
                table: "CustomerUserProfiles");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerUserProfiles");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "TenantProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "ServicePackages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ContractStatuses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ContractId",
                table: "ServicePackages",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePackages_Contracts_ContractId",
                table: "ServicePackages",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
