using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedConstraintTypeAddDataEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConstraintTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65"), "Закупки" },
                    { new Guid("5ef7dd65-e63f-4d94-a270-4a2d4c74728e"), "БУХУЧЕТ" },
                    { new Guid("63aa36f1-5185-4d77-9fd8-02f78c062808"), "Торги" },
                    { new Guid("6d562221-4437-48d7-b17c-ba4c21328a86"), "НПА" },
                    { new Guid("8b1fe2cb-165c-449d-a005-71870f37085b"), "Претензии" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65"));

            migrationBuilder.DeleteData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5ef7dd65-e63f-4d94-a270-4a2d4c74728e"));

            migrationBuilder.DeleteData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("63aa36f1-5185-4d77-9fd8-02f78c062808"));

            migrationBuilder.DeleteData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("6d562221-4437-48d7-b17c-ba4c21328a86"));

            migrationBuilder.DeleteData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("8b1fe2cb-165c-449d-a005-71870f37085b"));
        }
    }
}
