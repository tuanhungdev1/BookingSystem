using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHomestayColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableRooms",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeCancellationDays",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreeCancellation",
                table: "Homestays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrepaymentRequired",
                table: "Homestays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoomsAtThisPrice",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableRooms",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "FreeCancellationDays",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "IsFreeCancellation",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "IsPrepaymentRequired",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "RoomsAtThisPrice",
                table: "Homestays");
        }
    }
}
