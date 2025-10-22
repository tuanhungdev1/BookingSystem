using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableHomestayAmenity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsHighlight",
                table: "HomestayAmenities",
                newName: "IsDeleted");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HomestayRules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HomestayRules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HomestayRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HomestayRules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "HomestayRules",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HomestayRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HomestayRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "HomestayRules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HomestayAmenities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HomestayAmenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HomestayAmenities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HomestayAmenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "HomestayAmenities",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HomestayAmenities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "HomestayAmenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "HomestayRules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "HomestayAmenities");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "HomestayAmenities",
                newName: "IsHighlight");
        }
    }
}
