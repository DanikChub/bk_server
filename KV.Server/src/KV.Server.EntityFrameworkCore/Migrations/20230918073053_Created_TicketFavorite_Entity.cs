using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedTicketFavoriteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketClientFavorites",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketClientFavorites", x => new { x.ClientId, x.TicketId });
                    table.ForeignKey(
                        name: "FK_TicketClientFavorites_CustomerUserProfiles_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CustomerUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketClientFavorites_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketSpecialistFavorites",
                columns: table => new
                {
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSpecialistFavorites", x => new { x.SpecialistId, x.TicketId });
                    table.ForeignKey(
                        name: "FK_TicketSpecialistFavorites_CustomerUserProfiles_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "CustomerUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketSpecialistFavorites_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketClientFavorites_TicketId",
                table: "TicketClientFavorites",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSpecialistFavorites_TicketId",
                table: "TicketSpecialistFavorites",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketClientFavorites");

            migrationBuilder.DropTable(
                name: "TicketSpecialistFavorites");
        }
    }
}
