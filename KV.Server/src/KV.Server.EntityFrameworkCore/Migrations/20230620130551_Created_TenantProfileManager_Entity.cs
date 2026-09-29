using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedTenantProfileManagerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantProfileManagers",
                columns: table => new
                {
                    TenantProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantProfileManagers", x => new { x.TenantProfileId, x.ManagerId });
                    table.ForeignKey(
                        name: "FK_TenantProfileManagers_CustomerUserProfiles_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "CustomerUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantProfileManagers_TenantProfiles_TenantProfileId",
                        column: x => x.TenantProfileId,
                        principalTable: "TenantProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfileManagers_ManagerId",
                table: "TenantProfileManagers",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantProfileManagers");
        }
    }
}
