using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedTicketHoursSpentHistoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TicketHoursSpentHistoryId",
                table: "TicketHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketHoursSpentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHoursSpentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketHoursSpentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Spent = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHoursSpentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHoursSpentHistories_TicketHistory_TicketHistoryId",
                        column: x => x.TicketHistoryId,
                        principalTable: "TicketHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketHoursSpentHistories_TicketHoursSpentTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TicketHoursSpentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHoursSpentHistories_TicketHistoryId",
                table: "TicketHoursSpentHistories",
                column: "TicketHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketHoursSpentHistories_TypeId",
                table: "TicketHoursSpentHistories",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketHoursSpentHistories");

            migrationBuilder.DropTable(
                name: "TicketHoursSpentTypes");

            migrationBuilder.DropColumn(
                name: "TicketHoursSpentHistoryId",
                table: "TicketHistory");
        }
    }
}
