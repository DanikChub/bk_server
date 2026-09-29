using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedTicketMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TicketId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_CustomerUserProfiles_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "CustomerUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_CreatorId",
                table: "TicketHistory",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_CreatorId",
                table: "TicketMessages",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistory_AbpUsers_CreatorId",
                table: "TicketHistory",
                column: "CreatorId",
                principalTable: "AbpUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistory_AbpUsers_CreatorId",
                table: "TicketHistory");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketHistory_CreatorId",
                table: "TicketHistory");
        }
    }
}
