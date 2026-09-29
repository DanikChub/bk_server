using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    public partial class Created_ServicePackage_And_Contract_Entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "ContractDate",
                table: "Contracts",
                newName: "ContractStartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "UploadedFiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "UploadedFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "UploadedFiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "UploadedFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageProvider",
                table: "UploadedFiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractFinishDate",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackages_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistoryAttachments",
                columns: table => new
                {
                    UploadedFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketHistoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistoryAttachments", x => new { x.TicketHistoryId, x.UploadedFileId });
                    table.ForeignKey(
                        name: "FK_TicketHistoryAttachments_TicketHistory_TicketHistoryId",
                        column: x => x.TicketHistoryId,
                        principalTable: "TicketHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketHistoryAttachments_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractServicePackages",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContractFinishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractServicePackages", x => new { x.ContractId, x.ServicePackageId });
                    table.ForeignKey(
                        name: "FK_ContractServicePackages_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractServicePackages_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractServicePackages_ServicePackageId",
                table: "ContractServicePackages",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ContractId",
                table: "ServicePackages",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistoryAttachments_UploadedFileId",
                table: "TicketHistoryAttachments",
                column: "UploadedFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractServicePackages");

            migrationBuilder.DropTable(
                name: "TicketHistoryAttachments");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "StorageProvider",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "ContractFinishDate",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "ContractStartDate",
                table: "Contracts",
                newName: "ContractDate");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "UploadedFiles",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Tickets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
