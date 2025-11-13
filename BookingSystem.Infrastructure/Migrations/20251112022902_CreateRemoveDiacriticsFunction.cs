using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateRemoveDiacriticsFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([City])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([Country])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "FullAddressNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([FullAddress])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "HomestayDescriptionNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([HomestayDescription])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "HomestayTitleNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([HomestayTitle])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([Province])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchKeywordsNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([SearchKeywords])",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SlugNormalized",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "dbo.RemoveDiacritics([Slug])",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "CountryNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "FullAddressNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "HomestayDescriptionNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "HomestayTitleNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "ProvinceNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "SearchKeywordsNormalized",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "SlugNormalized",
                table: "Homestays");
        }
    }
}
