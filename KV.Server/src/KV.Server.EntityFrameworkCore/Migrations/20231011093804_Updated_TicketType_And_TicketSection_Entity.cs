using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketTypeAndTicketSectionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "SLATime",
                table: "TicketTypes",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.InsertData(
                table: "TicketSections",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a417"), "Бухгалтерский учет" },
                    { new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданское законодательство" },
                    { new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a417"), "Административное законодательство" },
                    { new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a417"), "Бюджетное законодательство" },
                    { new Guid("5736e1b2-3ad6-471f-971f-d2a27f86a417"), "Полномочия органов власти(ОМСУ)" },
                    { new Guid("6736e1b2-3ad6-471f-971f-d2a27f86a417"), "Трудовое законодательство" },
                    { new Guid("7736e1b2-3ad6-471f-971f-d2a27f86a417"), "Земельное законодательство" },
                    { new Guid("8736e1b2-3ad6-471f-971f-d2a27f86a417"), "Муниципальное имущество" },
                    { new Guid("9736e1b2-3ad6-471f-971f-d2a27f86a417"), "Закупки(44 - ФЗ, 223 - ФЗ)" },
                    { new Guid("a036e1b2-3ad6-471f-971f-d2a27f86a417"), "Налоговое законодательство" },
                    { new Guid("b136e1b2-3ad6-471f-971f-d2a27f86a417"), "Противодействие коррупции" },
                    { new Guid("c236e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданский / Арбитражный процесс, КАС" },
                    { new Guid("d336e1b2-3ad6-471f-971f-d2a27f86a417"), "Торги(Аренда, Продажа, Отбор УК)" },
                    { new Guid("e736e1b2-3ad6-471f-971f-d2a27f86a417"), "Антимонопольное законодательство" },
                    { new Guid("f736e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданская / Муниципальная служба" },
                    { new Guid("fa36e1b2-3ad6-471f-971f-d2a27f86a417"), "НТО" },
                    { new Guid("fb36e1b2-3ad6-471f-971f-d2a27f86a417"), "Корпоративное право" },
                    { new Guid("fc36e1b2-3ad6-471f-971f-d2a27f86a417"), "Иное" }
                });

            migrationBuilder.UpdateData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a416"),
                column: "SLATime",
                value: new TimeSpan(2, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a417"),
                column: "SLATime",
                value: new TimeSpan(2, 0, 0, 0, 0));

            migrationBuilder.InsertData(
                table: "TicketTypes",
                columns: new[] { "Id", "Name", "SLATime" },
                values: new object[,]
                {
                    { new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a416"), "Размещение", new TimeSpan(7, 0, 0, 0, 0) },
                    { new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a416"), "Разработка", new TimeSpan(7, 0, 0, 0, 0) },
                    { new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a416"), "Сайт", new TimeSpan(2, 0, 0, 0, 0) },
                    { new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a416"), "ИТ", new TimeSpan(2, 0, 0, 0, 0) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("5736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("6736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("7736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("8736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("9736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("a036e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("b136e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("c236e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("d336e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("e736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("f736e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("fa36e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("fb36e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketSections",
                keyColumn: "Id",
                keyValue: new Guid("fc36e1b2-3ad6-471f-971f-d2a27f86a417"));

            migrationBuilder.DeleteData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a416"));

            migrationBuilder.DeleteData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a416"));

            migrationBuilder.DeleteData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a416"));

            migrationBuilder.DeleteData(
                table: "TicketTypes",
                keyColumn: "Id",
                keyValue: new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a416"));

            migrationBuilder.DropColumn(
                name: "SLATime",
                table: "TicketTypes");
        }
    }
}
