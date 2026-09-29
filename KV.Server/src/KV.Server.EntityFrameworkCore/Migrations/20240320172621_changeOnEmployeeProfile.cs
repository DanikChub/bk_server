using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeOnEmployeeProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ResponsibleManagerId",
                table: "TenantProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_ResponsibleId",
                table: "Tickets");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerUserProfileId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Providers",
                table: "AbpSettingDefinitions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultValue",
                table: "AbpSettingDefinitions",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerUserProfileId",
                table: "Tickets",
                column: "CustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_EmployeeProfiles_ResponsibleManagerId",
                table: "TenantProfiles",
                column: "ResponsibleManagerId",
                principalTable: "EmployeeProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_CustomerUserProfileId",
                table: "Tickets",
                column: "CustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_EmployeeProfiles_ResponsibleId",
                table: "Tickets",
                column: "ResponsibleId",
                principalTable: "EmployeeProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_EmployeeProfiles_ResponsibleManagerId",
                table: "TenantProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_EmployeeProfiles_ResponsibleId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "Providers",
                table: "AbpSettingDefinitions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultValue",
                table: "AbpSettingDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ResponsibleManagerId",
                table: "TenantProfiles",
                column: "ResponsibleManagerId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_ResponsibleId",
                table: "Tickets",
                column: "ResponsibleId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");
        }
    }
}
