using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketStatusAndServicePackageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayNameMany",
                table: "TicketStatuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Style",
                table: "TicketStatuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Style",
                table: "ServicePackages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayNameMany",
                table: "TicketStatuses");

            migrationBuilder.DropColumn(
                name: "Style",
                table: "TicketStatuses");

            migrationBuilder.DropColumn(
                name: "Style",
                table: "ServicePackages");
        }
    }
}
