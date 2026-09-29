using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEventAddTicketIdEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TicketId",
                table: "Events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_TicketId",
                table: "Events",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Tickets_TicketId",
                table: "Events",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Tickets_TicketId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_TicketId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "Events");
        }
    }
}
