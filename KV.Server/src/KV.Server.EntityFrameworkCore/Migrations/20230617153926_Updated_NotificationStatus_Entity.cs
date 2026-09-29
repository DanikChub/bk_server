using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNotificationStatusEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_NotificationStatusId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_NotificationStatusId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "NotificationStatusId",
                table: "UserNotifications");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NotificationStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "NotificationStatuses");

            migrationBuilder.AddColumn<Guid>(
                name: "NotificationStatusId",
                table: "UserNotifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationStatusId",
                table: "UserNotifications",
                column: "NotificationStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_NotificationStatusId",
                table: "UserNotifications",
                column: "NotificationStatusId",
                principalTable: "NotificationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
