using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketMessageAddRecipientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_CreatorId",
                table: "TicketMessages");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "TicketMessages",
                newName: "RecipientCustomerUserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessages_CreatorId",
                table: "TicketMessages",
                newName: "IX_TicketMessages_RecipientCustomerUserProfileId");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorCustomerUserProfileId",
                table: "TicketMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_CreatorCustomerUserProfileId",
                table: "TicketMessages",
                column: "CreatorCustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_CreatorCustomerUserProf~",
                table: "TicketMessages",
                column: "CreatorCustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_RecipientCustomerUserPr~",
                table: "TicketMessages",
                column: "RecipientCustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_CreatorCustomerUserProf~",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_RecipientCustomerUserPr~",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_CreatorCustomerUserProfileId",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "CreatorCustomerUserProfileId",
                table: "TicketMessages");

            migrationBuilder.RenameColumn(
                name: "RecipientCustomerUserProfileId",
                table: "TicketMessages",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessages_RecipientCustomerUserProfileId",
                table: "TicketMessages",
                newName: "IX_TicketMessages_CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_CreatorId",
                table: "TicketMessages",
                column: "CreatorId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
