using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketTypeAddDataEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TicketTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a416"), "Устная" }
                });

            migrationBuilder.UpdateData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a417"),
                column: "Name",
                value: "Письменная");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a416"));
        }
    }
}
