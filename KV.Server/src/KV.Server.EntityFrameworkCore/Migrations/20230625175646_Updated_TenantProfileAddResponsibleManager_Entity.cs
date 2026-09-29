using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTenantProfileAddResponsibleManagerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleManagerId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_ResponsibleManagerId",
                table: "TenantProfiles",
                column: "ResponsibleManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ResponsibleManagerId",
                table: "TenantProfiles",
                column: "ResponsibleManagerId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ResponsibleManagerId",
                table: "TenantProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TenantProfiles_ResponsibleManagerId",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ResponsibleManagerId",
                table: "TenantProfiles");
        }
    }
}
