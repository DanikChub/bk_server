using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class removeIsUniqueFromContractStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractStatuses_Code",
                table: "ContractStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ContractStatuses_Title",
                table: "ContractStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_Code",
                table: "ContractStatuses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_Title",
                table: "ContractStatuses",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractStatuses_Code",
                table: "ContractStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ContractStatuses_Title",
                table: "ContractStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_Code",
                table: "ContractStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_Title",
                table: "ContractStatuses",
                column: "Title",
                unique: true);
        }
    }
}
