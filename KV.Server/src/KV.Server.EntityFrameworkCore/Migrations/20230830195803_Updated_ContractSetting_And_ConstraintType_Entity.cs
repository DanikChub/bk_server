using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedContractSettingAndConstraintTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursRangeTypes_TicketHours~",
                table: "TicketHoursSpentHistories");

            migrationBuilder.DropTable(
                name: "TicketHoursRangeTypes");

            migrationBuilder.RenameColumn(
                name: "TicketHoursRangeTypeId",
                table: "TicketHoursSpentHistories",
                newName: "ConstraintTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHoursSpentHistories_TicketHoursRangeTypeId",
                table: "TicketHoursSpentHistories",
                newName: "IX_TicketHoursSpentHistories_ConstraintTypeId");

            migrationBuilder.CreateTable(
                name: "ConstraintTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstraintTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Max = table.Column<int>(type: "integer", nullable: false),
                    ConstraintTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractSettings_ConstraintTypes_ConstraintTypeId",
                        column: x => x.ConstraintTypeId,
                        principalTable: "ConstraintTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractSettings_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSettings_ConstraintTypeId",
                table: "ContractSettings",
                column: "ConstraintTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSettings_ContractId",
                table: "ContractSettings",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHoursSpentHistories_ConstraintTypes_ConstraintTypeId",
                table: "TicketHoursSpentHistories",
                column: "ConstraintTypeId",
                principalTable: "ConstraintTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketHoursSpentHistories_ConstraintTypes_ConstraintTypeId",
                table: "TicketHoursSpentHistories");

            migrationBuilder.DropTable(
                name: "ContractSettings");

            migrationBuilder.DropTable(
                name: "ConstraintTypes");

            migrationBuilder.RenameColumn(
                name: "ConstraintTypeId",
                table: "TicketHoursSpentHistories",
                newName: "TicketHoursRangeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketHoursSpentHistories_ConstraintTypeId",
                table: "TicketHoursSpentHistories",
                newName: "IX_TicketHoursSpentHistories_TicketHoursRangeTypeId");

            migrationBuilder.CreateTable(
                name: "TicketHoursRangeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHoursRangeTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHoursSpentHistories_TicketHoursRangeTypes_TicketHours~",
                table: "TicketHoursSpentHistories",
                column: "TicketHoursRangeTypeId",
                principalTable: "TicketHoursRangeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
