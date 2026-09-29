using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketAddDeadlineEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Tickets",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2022, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Tickets");
        }
    }
}
