using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedStoryGroupNameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Stories",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Stories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Stories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Stories",
                newName: "Type");
        }
    }
}
