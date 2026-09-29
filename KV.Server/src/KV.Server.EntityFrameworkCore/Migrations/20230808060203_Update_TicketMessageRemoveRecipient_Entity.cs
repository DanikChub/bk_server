using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketMessageRemoveRecipientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_RecipientCustomerUserPr~",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_RecipientCustomerUserProfileId",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "RecipientCustomerUserProfileId",
                table: "TicketMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecipientCustomerUserProfileId",
                table: "TicketMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_RecipientCustomerUserProfileId",
                table: "TicketMessages",
                column: "RecipientCustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_CustomerUserProfiles_RecipientCustomerUserPr~",
                table: "TicketMessages",
                column: "RecipientCustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
