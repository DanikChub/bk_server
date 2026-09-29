using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketAddReadByWithDateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReadByClientDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadBySpecialistDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadByClientDate",
                table: "TicketHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadBySpecialistDate",
                table: "TicketHistory",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadByClientDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReadBySpecialistDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReadByClientDate",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "ReadBySpecialistDate",
                table: "TicketHistory");
        }
    }
}
