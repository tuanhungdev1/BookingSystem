using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHomestayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNote",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AreaInSquareMeters",
                table: "Homestays",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingCount",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasParking",
                table: "Homestays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPrivatePool",
                table: "Homestays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPetFriendly",
                table: "Homestays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumChildren",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFloors",
                table: "Homestays",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RatingAverage",
                table: "Homestays",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchKeywords",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Homestays",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalNote",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "AreaInSquareMeters",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "BookingCount",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "HasParking",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "HasPrivatePool",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "IsPetFriendly",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "MaximumChildren",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "NumberOfFloors",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "SearchKeywords",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Homestays");
        }
    }
}
