using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAccommodationHotelIdtoAccommodationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelImages_Accommodations_HotelId",
                table: "HotelImages");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "HotelImages",
                newName: "AccommodationId");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelImages_Accommodations_AccommodationId",
                table: "HotelImages",
                column: "AccommodationId",
                principalTable: "Accommodations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelImages_Accommodations_AccommodationId",
                table: "HotelImages");

            migrationBuilder.RenameColumn(
                name: "AccommodationId",
                table: "HotelImages",
                newName: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelImages_Accommodations_HotelId",
                table: "HotelImages",
                column: "HotelId",
                principalTable: "Accommodations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
