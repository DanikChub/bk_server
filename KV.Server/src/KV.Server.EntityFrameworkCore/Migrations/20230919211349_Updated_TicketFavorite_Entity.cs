using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketFavoriteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketSpecialistFavorites_CustomerUserProfiles_SpecialistId",
                table: "TicketSpecialistFavorites");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerUserProfileId",
                table: "TicketSpecialistFavorites",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketSpecialistFavorites_CustomerUserProfileId",
                table: "TicketSpecialistFavorites",
                column: "CustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSpecialistFavorites_AbpUsers_SpecialistId",
                table: "TicketSpecialistFavorites",
                column: "SpecialistId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSpecialistFavorites_CustomerUserProfiles_CustomerUser~",
                table: "TicketSpecialistFavorites",
                column: "CustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketSpecialistFavorites_AbpUsers_SpecialistId",
                table: "TicketSpecialistFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketSpecialistFavorites_CustomerUserProfiles_CustomerUser~",
                table: "TicketSpecialistFavorites");

            migrationBuilder.DropIndex(
                name: "IX_TicketSpecialistFavorites_CustomerUserProfileId",
                table: "TicketSpecialistFavorites");

            migrationBuilder.DropColumn(
                name: "CustomerUserProfileId",
                table: "TicketSpecialistFavorites");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSpecialistFavorites_CustomerUserProfiles_SpecialistId",
                table: "TicketSpecialistFavorites",
                column: "SpecialistId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
