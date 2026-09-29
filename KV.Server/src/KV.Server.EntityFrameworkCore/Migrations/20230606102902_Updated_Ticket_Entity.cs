using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ContractId",
                table: "Tickets",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Contracts_ContractId",
                table: "Tickets",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Contracts_ContractId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ContractId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Tickets");
        }
    }
}
