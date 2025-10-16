using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseforproject1_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Amenities_IsActive_IsPopular",
                table: "Amenities");

            migrationBuilder.DropColumn(
                name: "IsPopular",
                table: "Amenities");

            migrationBuilder.AddColumn<string>(
                name: "CustomNote",
                table: "HomestayAmenities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlight",
                table: "HomestayAmenities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommon",
                table: "Amenities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "Amenities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CouponName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CouponType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalUsageLimit = table.Column<int>(type: "int", nullable: true),
                    CurrentUsageCount = table.Column<int>(type: "int", nullable: false),
                    UsagePerUser = table.Column<int>(type: "int", nullable: true),
                    MinimumBookingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinimumNights = table.Column<int>(type: "int", nullable: true),
                    IsFirstBookingOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsNewUserOnly = table.Column<bool>(type: "bit", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    SpecificHomestayId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_Homestays_SpecificHomestayId",
                        column: x => x.SpecificHomestayId,
                        principalTable: "Homestays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coupons_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HostProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AboutMe = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Languages = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankAccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdentityCardFrontUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentityCardBackUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessLicenseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCodeDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalHomestays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalBookings = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0m),
                    ResponseRate = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageResponseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSuperhost = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RegisteredAsHostAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicantNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostProfiles_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HostProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponHomestays",
                columns: table => new
                {
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    HomestayId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponHomestays", x => new { x.CouponId, x.HomestayId });
                    table.ForeignKey(
                        name: "FK_CouponHomestays_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponHomestays_Homestays_HomestayId",
                        column: x => x.HomestayId,
                        principalTable: "Homestays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponHomestays_HomestayId",
                table: "CouponHomestays",
                column: "HomestayId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CouponCode",
                table: "Coupons",
                column: "CouponCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CreatedByUserId",
                table: "Coupons",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_EndDate",
                table: "Coupons",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive",
                table: "Coupons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_SpecificHomestayId",
                table: "Coupons",
                column: "SpecificHomestayId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_StartDate",
                table: "Coupons",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_BookingId",
                table: "CouponUsages",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId",
                table: "CouponUsages",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UsedAt",
                table: "CouponUsages",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HostProfiles_ReviewedByAdminId",
                table: "HostProfiles",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_HostProfiles_UserId",
                table: "HostProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponHomestays");

            migrationBuilder.DropTable(
                name: "CouponUsages");

            migrationBuilder.DropTable(
                name: "HostProfiles");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropColumn(
                name: "CustomNote",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "IsHighlight",
                table: "HomestayAmenities");

            migrationBuilder.DropColumn(
                name: "IsCommon",
                table: "Amenities");

            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "Amenities");

            migrationBuilder.AddColumn<bool>(
                name: "IsPopular",
                table: "Amenities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_IsActive_IsPopular",
                table: "Amenities",
                columns: new[] { "IsActive", "IsPopular" });
        }
    }
}
