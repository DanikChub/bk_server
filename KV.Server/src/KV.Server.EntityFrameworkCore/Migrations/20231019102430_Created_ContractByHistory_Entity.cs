using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatedContractByHistoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractConstraintsByMonths",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConstraintTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    MaxCount = table.Column<int>(type: "integer", nullable: false),
                    Sum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractConstraintsByMonths", x => new { x.ContractId, x.ConstraintTypeId });
                    table.ForeignKey(
                        name: "FK_ContractConstraintsByMonths_ConstraintTypes_ConstraintTypeId",
                        column: x => x.ConstraintTypeId,
                        principalTable: "ConstraintTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractConstraintsByMonths_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractTicketsByTicketSectionByMonths",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTicketsByTicketSectionByMonths", x => new { x.ContractId, x.TicketSectionId });
                    table.ForeignKey(
                        name: "FK_ContractTicketsByTicketSectionByMonths_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractTicketsByTicketSectionByMonths_TicketSections_Ticke~",
                        column: x => x.TicketSectionId,
                        principalTable: "TicketSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractTicketsByTicketTypeByMonths",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTicketsByTicketTypeByMonths", x => new { x.ContractId, x.TicketTypeId });
                    table.ForeignKey(
                        name: "FK_ContractTicketsByTicketTypeByMonths_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractTicketsByTicketTypeByMonths_TicketTypes_TicketTypeId",
                        column: x => x.TicketTypeId,
                        principalTable: "TicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractConstraintsByMonths_ConstraintTypeId",
                table: "ContractConstraintsByMonths",
                column: "ConstraintTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTicketsByTicketSectionByMonths_TicketSectionId",
                table: "ContractTicketsByTicketSectionByMonths",
                column: "TicketSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTicketsByTicketTypeByMonths_TicketTypeId",
                table: "ContractTicketsByTicketTypeByMonths",
                column: "TicketTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractConstraintsByMonths");

            migrationBuilder.DropTable(
                name: "ContractTicketsByTicketSectionByMonths");

            migrationBuilder.DropTable(
                name: "ContractTicketsByTicketTypeByMonths");
        }
    }
}
