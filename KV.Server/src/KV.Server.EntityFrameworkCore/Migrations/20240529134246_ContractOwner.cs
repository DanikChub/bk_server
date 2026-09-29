using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class ContractOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractEmail",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ContractOwner",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ContractPhoneNumber",
                table: "TenantProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactUserProfileId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_ContactUserProfileId",
                table: "TenantProfiles",
                column: "ContactUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatorId",
                table: "Events",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_DeleterId",
                table: "Events",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_LastModifierId",
                table: "Events",
                column: "LastModifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AbpUsers_CreatorId",
                table: "Events",
                column: "CreatorId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AbpUsers_DeleterId",
                table: "Events",
                column: "DeleterId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AbpUsers_LastModifierId",
                table: "Events",
                column: "LastModifierId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ContactUserProfileId",
                table: "TenantProfiles",
                column: "ContactUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AbpUsers_CreatorId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_AbpUsers_DeleterId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_AbpUsers_LastModifierId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantProfiles_CustomerUserProfiles_ContactUserProfileId",
                table: "TenantProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TenantProfiles_ContactUserProfileId",
                table: "TenantProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Events_CreatorId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_DeleterId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_LastModifierId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ContactUserProfileId",
                table: "TenantProfiles");

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
        }
    }
}
