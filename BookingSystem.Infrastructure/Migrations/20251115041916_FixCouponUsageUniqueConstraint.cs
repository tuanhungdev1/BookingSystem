using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCouponUsageUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_BookingId",
                table: "CouponUsages");

            migrationBuilder.AddColumn<int>(
                name: "BookingId1",
                table: "CouponUsages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_BookingId",
                table: "CouponUsages",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_BookingId1",
                table: "CouponUsages",
                column: "BookingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_Bookings_BookingId1",
                table: "CouponUsages",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_Bookings_BookingId1",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_BookingId",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_BookingId1",
                table: "CouponUsages");

            migrationBuilder.DropColumn(
                name: "BookingId1",
                table: "CouponUsages");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_BookingId",
                table: "CouponUsages",
                column: "BookingId",
                unique: true);
        }
    }
}
