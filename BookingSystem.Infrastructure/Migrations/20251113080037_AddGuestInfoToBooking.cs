using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestInfoToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualGuestEmail",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualGuestFullName",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualGuestIdNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualGuestNotes",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualGuestPhoneNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAddress",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestCity",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestCountry",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuestFullName",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuestPhoneNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBookingForSomeoneElse",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualGuestEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ActualGuestFullName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ActualGuestIdNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ActualGuestNotes",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ActualGuestPhoneNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestAddress",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestCity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestCountry",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestFullName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestPhoneNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsBookingForSomeoneElse",
                table: "Bookings");
        }
    }
}
