using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfBirthDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAvatarFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobPost = table.Column<string>(type: "text", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_AbpUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_UploadedFiles_UserAvatarFileId",
                        column: x => x.UserAvatarFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_IdentityUserId",
                table: "EmployeeProfiles",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_UserAvatarFileId",
                table: "EmployeeProfiles",
                column: "UserAvatarFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeProfiles");
        }
    }
}
