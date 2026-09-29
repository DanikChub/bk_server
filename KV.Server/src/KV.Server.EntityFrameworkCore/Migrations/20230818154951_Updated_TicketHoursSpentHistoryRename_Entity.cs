using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketHoursSpentHistoryRenameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursSpentTypes_TypeId",
                table: "TicketHoursSpentHistories");

            migrationBuilder.DropTable(
                name: "TicketHoursSpentTypes");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "TicketHoursSpentHistories",
                newName: "TicketHoursRangeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHoursSpentHistories_TypeId",
                table: "TicketHoursSpentHistories",
                newName: "IX_TicketHoursSpentHistories_TicketHoursRangeTypeId");

            migrationBuilder.CreateTable(
                name: "TicketHoursRangeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHoursRangeTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursRangeTypes_TicketHours~",
                table: "TicketHoursSpentHistories",
                column: "TicketHoursRangeTypeId",
                principalTable: "TicketHoursRangeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursRangeTypes_TicketHours~",
                table: "TicketHoursSpentHistories");

            migrationBuilder.DropTable(
                name: "TicketHoursRangeTypes");

            migrationBuilder.RenameColumn(
                name: "TicketHoursRangeTypeId",
                table: "TicketHoursSpentHistories",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHoursSpentHistories_TicketHoursRangeTypeId",
                table: "TicketHoursSpentHistories",
                newName: "IX_TicketHoursSpentHistories_TypeId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursSpentTypes_TypeId",
                table: "TicketHoursSpentHistories",
                column: "TypeId",
                principalTable: "TicketHoursSpentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
