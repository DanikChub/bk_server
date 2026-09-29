using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedNotificationStatusAndCategoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_CategoryNotifications_CategoryNotificatio~",
                table: "UserNotifications");

            migrationBuilder.DropTable(
                name: "CategoryNotifications");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusNotificationId",
                table: "UserNotifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "NotificationCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Style = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Style = table.Column<string>(type: "text", nullable: true),
                    DisplayNameMany = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_StatusNotificationId",
                table: "UserNotifications",
                column: "StatusNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationCategories_CategoryNotificati~",
                table: "UserNotifications",
                column: "CategoryNotificationId",
                principalTable: "NotificationCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_StatusNotificationId",
                table: "UserNotifications",
                column: "StatusNotificationId",
                principalTable: "NotificationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationCategories_CategoryNotificati~",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_StatusNotificationId",
                table: "UserNotifications");

            migrationBuilder.DropTable(
                name: "NotificationCategories");

            migrationBuilder.DropTable(
                name: "NotificationStatuses");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_StatusNotificationId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "StatusNotificationId",
                table: "UserNotifications");

            migrationBuilder.CreateTable(
                name: "CategoryNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNotifications", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_CategoryNotifications_CategoryNotificatio~",
                table: "UserNotifications",
                column: "CategoryNotificationId",
                principalTable: "CategoryNotifications",
                principalColumn: "Id");
        }
    }
}
