using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserNotificationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_CustomerUserProfiles_CustomerUserProfileId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationCategories_CategoryNotificati~",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_StatusNotificationId",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "StatusNotificationId",
                table: "UserNotifications",
                newName: "NotificationStatusId");

            migrationBuilder.RenameColumn(
                name: "CustomerUserProfileId",
                table: "UserNotifications",
                newName: "IdentityUserId");

            migrationBuilder.RenameColumn(
                name: "CategoryNotificationId",
                table: "UserNotifications",
                newName: "NotificationCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_StatusNotificationId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_NotificationStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_CustomerUserProfileId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_IdentityUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_CategoryNotificationId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_NotificationCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "UserNotifications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_CreatorId",
                table: "UserNotifications",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_AbpUsers_CreatorId",
                table: "UserNotifications",
                column: "CreatorId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_AbpUsers_IdentityUserId",
                table: "UserNotifications",
                column: "IdentityUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationCategories_NotificationCatego~",
                table: "UserNotifications",
                column: "NotificationCategoryId",
                principalTable: "NotificationCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_NotificationStatusId",
                table: "UserNotifications",
                column: "NotificationStatusId",
                principalTable: "NotificationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_AbpUsers_CreatorId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_AbpUsers_IdentityUserId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationCategories_NotificationCatego~",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_NotificationStatuses_NotificationStatusId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_CreatorId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "NotificationStatusId",
                table: "UserNotifications",
                newName: "StatusNotificationId");

            migrationBuilder.RenameColumn(
                name: "NotificationCategoryId",
                table: "UserNotifications",
                newName: "CategoryNotificationId");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "UserNotifications",
                newName: "CustomerUserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_NotificationStatusId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_StatusNotificationId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_NotificationCategoryId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_CategoryNotificationId");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_IdentityUserId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_CustomerUserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_CustomerUserProfiles_CustomerUserProfileId",
                table: "UserNotifications",
                column: "CustomerUserProfileId",
                principalTable: "CustomerUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
