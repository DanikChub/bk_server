using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedNavigationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CustomerUserProfileId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TenantProfiles_TenantId",
                table: "Tickets",
                column: "TenantId",
                principalTable: "TenantProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TenantProfiles_TenantId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerUserProfileId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerUserProfileId",
                table: "Tickets",
                column: "CustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_CustomerUserProfiles_CustomerUserProfileId",
                table: "Tickets",
                column: "CustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");
        }
    }
}
