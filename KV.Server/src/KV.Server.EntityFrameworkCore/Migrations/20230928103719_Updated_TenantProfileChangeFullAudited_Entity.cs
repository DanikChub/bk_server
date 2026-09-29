using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KV.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTenantProfileChangeFullAuditedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "TenantProfiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "TenantProfiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "TenantProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "TenantProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TenantProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "TenantProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "TenantProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65"),
                column: "Name",
                value: "НПА");

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5ef7dd65-e63f-4d94-a270-4a2d4c74728e"),
                column: "Name",
                value: "Закупки");

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("6d562221-4437-48d7-b17c-ba4c21328a86"),
                column: "Name",
                value: "БУХУЧЕТ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "TenantProfiles");

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65"),
                column: "Name",
                value: "Закупки");

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("5ef7dd65-e63f-4d94-a270-4a2d4c74728e"),
                column: "Name",
                value: "БУХУЧЕТ");

            migrationBuilder.UpdateData(
                table: "ConstraintTypes",
                keyColumn: "Id",
                keyValue: new Guid("6d562221-4437-48d7-b17c-ba4c21328a86"),
                column: "Name",
                value: "НПА");
        }
    }
}
