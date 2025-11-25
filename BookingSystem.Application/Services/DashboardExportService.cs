// ============================================================================
// DASHBOARD EXPORT SERVICE - COMPLETE IMPLEMENTATION
// ============================================================================

using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.DashboardDTO;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Microsoft.Extensions.Logging;
using System.Text;

using iTextDoc = iText.Layout.Document;
using iTextPara = iText.Layout.Element.Paragraph;
using iTextTable = iText.Layout.Element.Table;
using iTextCell = iText.Layout.Element.Cell;
using iText.Kernel.Geom;

namespace BookingSystem.Application.Services
{
	public class DashboardExportService : IDashboardExportService
	{
		private readonly ILogger<DashboardExportService> _logger;

		public DashboardExportService(ILogger<DashboardExportService> logger)
		{
			_logger = logger;
		}

		// ============================================================================
		// ADMIN DASHBOARD EXPORT - EXCEL
		// ============================================================================
		public async Task<byte[]> ExportAdminDashboardToExcelAsync(
			CompleteDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Admin Dashboard to Excel");

			try
			{
				using (var workbook = new XLWorkbook())
				{
					CreateAdminOverviewSheet(workbook, dashboard.Overview);
					CreateAdminUserStatisticsSheet(workbook, dashboard.UserStatistics);
					CreateAdminBookingStatisticsSheet(workbook, dashboard.BookingStatistics);
					CreateAdminRevenueStatisticsSheet(workbook, dashboard.RevenueStatistics);
					CreateAdminReviewStatisticsSheet(workbook, dashboard.ReviewStatistics);

					using (var memoryStream = new MemoryStream())
					{
						workbook.SaveAs(memoryStream);
						return memoryStream.ToArray();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting admin dashboard to Excel");
				throw new Exception("Failed to export admin dashboard to Excel", ex);
			}
		}

		// ============================================================================
		// ADMIN DASHBOARD EXPORT - PDF
		// ============================================================================
		public async Task<byte[]> ExportAdminDashboardToPdfAsync(
			CompleteDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Admin Dashboard to PDF");

			try
			{
				using var stream = new MemoryStream();
				using var writer = new PdfWriter(stream);
				using var pdf = new PdfDocument(writer);
				pdf.SetDefaultPageSize(PageSize.A4.Rotate());

				var document = new iTextDoc(pdf);
				document.SetMargins(30, 20, 30, 20);

				var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
				var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

				// Title
				document.Add(new iTextPara("ADMIN DASHBOARD REPORT")
					.SetFont(boldFont)
					.SetFontSize(20)
					.SetTextAlignment(TextAlignment.CENTER)
					.SetMarginBottom(5));

				document.Add(new iTextPara($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
					.SetFont(normalFont)
					.SetFontSize(9)
					.SetTextAlignment(TextAlignment.RIGHT)
					.SetMarginBottom(20));

				// Overview Table
				AddAdminOverviewPdfTable(document, dashboard.Overview, boldFont, normalFont);

				// User Statistics Table
				AddAdminUserStatisticsPdfTable(document, dashboard.UserStatistics, boldFont, normalFont);

				// Booking Statistics Table
				AddAdminBookingStatisticsPdfTable(document, dashboard.BookingStatistics, boldFont, normalFont);

				// Revenue Statistics Table
				AddAdminRevenueStatisticsPdfTable(document, dashboard.RevenueStatistics, boldFont, normalFont);

				document.Close();
				return stream.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting admin dashboard to PDF");
				throw new Exception("Failed to export admin dashboard to PDF", ex);
			}
		}

		// ============================================================================
		// ADMIN DASHBOARD EXPORT - CSV
		// ============================================================================
		public async Task<byte[]> ExportAdminDashboardToCSVAsync(
			CompleteDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Admin Dashboard to CSV");

			try
			{
				var sb = new StringBuilder();
				sb.Append('\ufeff');

				sb.AppendLine("ADMIN DASHBOARD REPORT");
				sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				sb.AppendLine();

				// Overview
				sb.AppendLine("OVERVIEW STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Users,{dashboard.Overview.TotalUsers}");
				sb.AppendLine($"Total Hosts,{dashboard.Overview.TotalHosts}");
				sb.AppendLine($"Total Homestays,{dashboard.Overview.TotalHomestays}");
				sb.AppendLine($"Active Homestays,{dashboard.Overview.ActiveHomestays}");
				sb.AppendLine($"Monthly Growth Rate,{dashboard.Overview.MonthlyGrowthRate}%");
				sb.AppendLine();

				// User Statistics
				sb.AppendLine("USER STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Daily Active Users,{dashboard.UserStatistics.DailyActiveUsers}");
				sb.AppendLine($"Monthly Active Users,{dashboard.UserStatistics.MonthlyActiveUsers}");
				sb.AppendLine($"Active Hosts,{dashboard.UserStatistics.ActiveHosts}");
				sb.AppendLine($"User to Host Conversion Rate,{dashboard.UserStatistics.UserToHostConversionRate}%");
				sb.AppendLine();

				// Users by Region
				sb.AppendLine("USERS BY REGION");
				sb.AppendLine("Region,User Count,Host Count,Percentage");
				foreach (var region in dashboard.UserStatistics.UsersByRegion)
				{
					sb.AppendLine($"{region.Region},{region.UserCount},{region.HostCount},{region.Percentage}%");
				}
				sb.AppendLine();

				// Booking Statistics
				sb.AppendLine("BOOKING STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Bookings,{dashboard.BookingStatistics.TotalBookings}");
				sb.AppendLine($"Completed Bookings,{dashboard.BookingStatistics.CompletedBookings}");
				sb.AppendLine($"Pending Bookings,{dashboard.BookingStatistics.PendingBookings}");
				sb.AppendLine($"Cancelled Bookings,{dashboard.BookingStatistics.CancelledBookings}");
				sb.AppendLine($"Occupancy Rate,{dashboard.BookingStatistics.OccupancyRate}%");
				sb.AppendLine();

				// Revenue Statistics
				sb.AppendLine("REVENUE STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Revenue,{dashboard.RevenueStatistics.TotalRevenue:N2}");
				sb.AppendLine($"Monthly Revenue,{dashboard.RevenueStatistics.MonthlyRevenue:N2}");
				sb.AppendLine($"Yearly Revenue,{dashboard.RevenueStatistics.YearlyRevenue:N2}");
				sb.AppendLine($"Average Revenue Per Booking,{dashboard.RevenueStatistics.AverageRevenuePerBooking:N2}");
				sb.AppendLine($"Refund Amount,{dashboard.RevenueStatistics.RefundAmount:N2}");
				sb.AppendLine($"Refund Rate,{dashboard.RevenueStatistics.RefundRate}%");
				sb.AppendLine();

				// Review Statistics
				sb.AppendLine("REVIEW STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Average Rating,{dashboard.ReviewStatistics.AverageRating}");
				sb.AppendLine($"Total Reviews,{dashboard.ReviewStatistics.TotalReviews}");
				sb.AppendLine($"New Reviews This Month,{dashboard.ReviewStatistics.NewReviewsThisMonth}");
				sb.AppendLine($"Complaint Count,{dashboard.ReviewStatistics.ComplaintCount}");

				return Encoding.UTF8.GetBytes(sb.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting admin dashboard to CSV");
				throw new Exception("Failed to export admin dashboard to CSV", ex);
			}
		}

		// ============================================================================
		// HOST DASHBOARD EXPORT - EXCEL
		// ============================================================================
		public async Task<byte[]> ExportHostDashboardToExcelAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Host Dashboard to Excel");

			try
			{
				using (var workbook = new XLWorkbook())
				{
					CreateHostOverviewSheet(workbook, dashboard.Overview);
					CreateHostRevenueSheet(workbook, dashboard.Revenue);
					CreateHostBookingSheet(workbook, dashboard.Bookings);
					CreateHostReviewSheet(workbook, dashboard.Reviews);
					CreateHostPerformanceSheet(workbook, dashboard.Performance);

					using (var memoryStream = new MemoryStream())
					{
						workbook.SaveAs(memoryStream);
						return memoryStream.ToArray();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting host dashboard to Excel");
				throw new Exception("Failed to export host dashboard to Excel", ex);
			}
		}

		// ============================================================================
		// HOST DASHBOARD EXPORT - PDF
		// ============================================================================
		public async Task<byte[]> ExportHostDashboardToPdfAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Host Dashboard to PDF");

			try
			{
				using var stream = new MemoryStream();
				using var writer = new PdfWriter(stream);
				using var pdf = new PdfDocument(writer);
				pdf.SetDefaultPageSize(PageSize.A4.Rotate());

				var document = new iTextDoc(pdf);
				document.SetMargins(30, 20, 30, 20);

				var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
				var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

				// Title
				document.Add(new iTextPara("HOST DASHBOARD REPORT")
					.SetFont(boldFont)
					.SetFontSize(20)
					.SetTextAlignment(TextAlignment.CENTER)
					.SetMarginBottom(5));

				document.Add(new iTextPara($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
					.SetFont(normalFont)
					.SetFontSize(9)
					.SetTextAlignment(TextAlignment.RIGHT)
					.SetMarginBottom(20));

				// Overview
				AddHostOverviewPdfTable(document, dashboard.Overview, boldFont, normalFont);

				// Revenue
				AddHostRevenueStatisticsPdfTable(document, dashboard.Revenue, boldFont, normalFont);

				// Bookings
				AddHostBookingStatisticsPdfTable(document, dashboard.Bookings, boldFont, normalFont);

				document.Close();
				return stream.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting host dashboard to PDF");
				throw new Exception("Failed to export host dashboard to PDF", ex);
			}
		}

		// ============================================================================
		// HOST DASHBOARD EXPORT - CSV
		// ============================================================================
		public async Task<byte[]> ExportHostDashboardToCSVAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12)
		{
			_logger.LogInformation("Exporting Host Dashboard to CSV");

			try
			{
				var sb = new StringBuilder();
				sb.Append('\ufeff');

				sb.AppendLine("HOST DASHBOARD REPORT");
				sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				sb.AppendLine();

				// Overview
				sb.AppendLine("OVERVIEW");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Homestays,{dashboard.Overview.TotalHomestays}");
				sb.AppendLine($"Active Homestays,{dashboard.Overview.ActiveHomestays}");
				sb.AppendLine($"Total Bookings,{dashboard.Overview.TotalBookings}");
				sb.AppendLine($"Total Revenue,{dashboard.Overview.TotalRevenue:N2}");
				sb.AppendLine($"Average Rating,{dashboard.Overview.AverageRating}");
				sb.AppendLine($"Total Reviews,{dashboard.Overview.TotalReviews}");
				sb.AppendLine($"Monthly Revenue,{dashboard.Overview.MonthlyRevenue:N2}");
				sb.AppendLine($"Revenue Growth,{dashboard.Overview.RevenueGrowth}%");
				sb.AppendLine();

				// Revenue
				sb.AppendLine("REVENUE STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Revenue,{dashboard.Revenue.TotalRevenue:N2}");
				sb.AppendLine($"Monthly Revenue,{dashboard.Revenue.MonthlyRevenue:N2}");
				sb.AppendLine($"Yearly Revenue,{dashboard.Revenue.YearlyRevenue:N2}");
				sb.AppendLine($"Average Booking Value,{dashboard.Revenue.AverageBookingValue:N2}");
				sb.AppendLine();

				// Booking
				sb.AppendLine("BOOKING STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Total Bookings,{dashboard.Bookings.TotalBookings}");
				sb.AppendLine($"Pending,{dashboard.Bookings.PendingBookings}");
				sb.AppendLine($"Confirmed,{dashboard.Bookings.ConfirmedBookings}");
				sb.AppendLine($"Completed,{dashboard.Bookings.CompletedBookings}");
				sb.AppendLine($"Cancelled,{dashboard.Bookings.CancelledBookings}");
				sb.AppendLine($"Cancellation Rate,{dashboard.Bookings.CancellationRate}%");
				sb.AppendLine($"Occupancy Rate,{dashboard.Bookings.OccupancyRate}%");
				sb.AppendLine();

				// Review
				sb.AppendLine("REVIEW STATISTICS");
				sb.AppendLine("Metric,Value");
				sb.AppendLine($"Average Rating,{dashboard.Reviews.AverageRating}");
				sb.AppendLine($"Total Reviews,{dashboard.Reviews.TotalReviews}");
				sb.AppendLine($"Average Cleanliness,{dashboard.Reviews.AverageCleanlinessRating}");
				sb.AppendLine($"Average Accuracy,{dashboard.Reviews.AverageAccuracyRating}");
				sb.AppendLine($"Average Communication,{dashboard.Reviews.AverageCommunicationRating}");
				sb.AppendLine($"Average Location,{dashboard.Reviews.AverageLocationRating}");
				sb.AppendLine($"Average Value,{dashboard.Reviews.AverageValueRating}");

				return Encoding.UTF8.GetBytes(sb.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting host dashboard to CSV");
				throw new Exception("Failed to export host dashboard to CSV", ex);
			}
		}

		// ============================================================================
		// HELPER METHODS - EXCEL SHEETS
		// ============================================================================

		private void CreateAdminOverviewSheet(XLWorkbook workbook, DashboardOverviewDto overview)
		{
			var ws = workbook.Worksheets.Add("Overview");
			FormatExcelHeader(ws, "ADMIN DASHBOARD OVERVIEW");

			int row = 3;
			AddExcelRow(ws, row++, "Total Users", overview.TotalUsers);
			AddExcelRow(ws, row++, "Total Hosts", overview.TotalHosts);
			AddExcelRow(ws, row++, "Total Homestays", overview.TotalHomestays);
			AddExcelRow(ws, row++, "Active Homestays", overview.ActiveHomestays);
			AddExcelRow(ws, row++, "Monthly Growth Rate", $"{overview.MonthlyGrowthRate}%");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateAdminUserStatisticsSheet(XLWorkbook workbook, DashboardUserStatisticsDto stats)
		{
			var ws = workbook.Worksheets.Add("User Statistics");
			FormatExcelHeader(ws, "USER STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Daily Active Users", stats.DailyActiveUsers);
			AddExcelRow(ws, row++, "Monthly Active Users", stats.MonthlyActiveUsers);
			AddExcelRow(ws, row++, "Active Hosts", stats.ActiveHosts);
			AddExcelRow(ws, row++, "Conversion Rate", $"{stats.UserToHostConversionRate}%");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateAdminBookingStatisticsSheet(XLWorkbook workbook, DashboardBookingStatisticsDto stats)
		{
			var ws = workbook.Worksheets.Add("Booking Statistics");
			FormatExcelHeader(ws, "BOOKING STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Total Bookings", stats.TotalBookings);
			AddExcelRow(ws, row++, "Completed", stats.CompletedBookings);
			AddExcelRow(ws, row++, "Pending", stats.PendingBookings);
			AddExcelRow(ws, row++, "Cancelled", stats.CancelledBookings);
			AddExcelRow(ws, row++, "Occupancy Rate", $"{stats.OccupancyRate}%");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateAdminRevenueStatisticsSheet(XLWorkbook workbook, RevenueStatisticsDto stats)
		{
			var ws = workbook.Worksheets.Add("Revenue Statistics");
			FormatExcelHeader(ws, "REVENUE STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Total Revenue", $"{stats.TotalRevenue:N2}");
			AddExcelRow(ws, row++, "Monthly Revenue", $"{stats.MonthlyRevenue:N2}");
			AddExcelRow(ws, row++, "Yearly Revenue", $"{stats.YearlyRevenue:N2}");
			AddExcelRow(ws, row++, "Avg Revenue/Booking", $"{stats.AverageRevenuePerBooking:N2}");
			AddExcelRow(ws, row++, "Refund Amount", $"{stats.RefundAmount:N2}");
			AddExcelRow(ws, row++, "Refund Rate", $"{stats.RefundRate}%");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateAdminReviewStatisticsSheet(XLWorkbook workbook, ReviewStatisticsDto stats)
		{
			var ws = workbook.Worksheets.Add("Review Statistics");
			FormatExcelHeader(ws, "REVIEW STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Average Rating", stats.AverageRating.ToString("F1"));
			AddExcelRow(ws, row++, "Total Reviews", stats.TotalReviews);
			AddExcelRow(ws, row++, "New This Month", stats.NewReviewsThisMonth);
			AddExcelRow(ws, row++, "Complaint Count", stats.ComplaintCount);

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		// Host Dashboard Sheets
		private void CreateHostOverviewSheet(XLWorkbook workbook, HostDashboardOverviewDto overview)
		{
			var ws = workbook.Worksheets.Add("Overview");
			FormatExcelHeader(ws, "HOST DASHBOARD OVERVIEW");

			int row = 3;
			AddExcelRow(ws, row++, "Total Homestays", overview.TotalHomestays);
			AddExcelRow(ws, row++, "Active Homestays", overview.ActiveHomestays);
			AddExcelRow(ws, row++, "Total Bookings", overview.TotalBookings);
			AddExcelRow(ws, row++, "Total Revenue", $"{overview.TotalRevenue:N2}");
			AddExcelRow(ws, row++, "Average Rating", overview.AverageRating.ToString("F1"));
			AddExcelRow(ws, row++, "Total Reviews", overview.TotalReviews);
			AddExcelRow(ws, row++, "Monthly Revenue", $"{overview.MonthlyRevenue:N2}");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateHostRevenueSheet(XLWorkbook workbook, HostRevenueStatisticsDto revenue)
		{
			var ws = workbook.Worksheets.Add("Revenue");
			FormatExcelHeader(ws, "HOST REVENUE STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Total Revenue", $"{revenue.TotalRevenue:N2}");
			AddExcelRow(ws, row++, "Monthly Revenue", $"{revenue.MonthlyRevenue:N2}");
			AddExcelRow(ws, row++, "Yearly Revenue", $"{revenue.YearlyRevenue:N2}");
			AddExcelRow(ws, row++, "Average Booking Value", $"{revenue.AverageBookingValue:N2}");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateHostBookingSheet(XLWorkbook workbook, HostBookingStatisticsDto bookings)
		{
			var ws = workbook.Worksheets.Add("Bookings");
			FormatExcelHeader(ws, "HOST BOOKING STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Total Bookings", bookings.TotalBookings);
			AddExcelRow(ws, row++, "Pending", bookings.PendingBookings);
			AddExcelRow(ws, row++, "Confirmed", bookings.ConfirmedBookings);
			AddExcelRow(ws, row++, "Completed", bookings.CompletedBookings);
			AddExcelRow(ws, row++, "Cancelled", bookings.CancelledBookings);
			AddExcelRow(ws, row++, "Cancellation Rate", $"{bookings.CancellationRate}%");
			AddExcelRow(ws, row++, "Occupancy Rate", $"{bookings.OccupancyRate}%");

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateHostReviewSheet(XLWorkbook workbook, HostReviewStatisticsDto reviews)
		{
			var ws = workbook.Worksheets.Add("Reviews");
			FormatExcelHeader(ws, "HOST REVIEW STATISTICS");

			int row = 3;
			AddExcelRow(ws, row++, "Average Rating", reviews.AverageRating.ToString("F1"));
			AddExcelRow(ws, row++, "Total Reviews", reviews.TotalReviews);
			AddExcelRow(ws, row++, "Cleanliness", reviews.AverageCleanlinessRating.ToString("F1"));
			AddExcelRow(ws, row++, "Accuracy", reviews.AverageAccuracyRating.ToString("F1"));
			AddExcelRow(ws, row++, "Communication", reviews.AverageCommunicationRating.ToString("F1"));
			AddExcelRow(ws, row++, "Location", reviews.AverageLocationRating.ToString("F1"));
			AddExcelRow(ws, row++, "Value", reviews.AverageValueRating.ToString("F1"));

			ws.Columns().AdjustToContents(minWidth: 15, maxWidth: 50);
		}

		private void CreateHostPerformanceSheet(XLWorkbook workbook, HostPerformanceDto performance)
		{
			var ws = workbook.Worksheets.Add("Performance");
			FormatExcelHeader(ws, "HOST PERFORMANCE");

			// Homestay Performance Table
			int row = 3;
			ws.Cell(row, 1).Value = "Homestay Title";
			ws.Cell(row, 2).Value = "Bookings";
			ws.Cell(row, 3).Value = "Revenue";
			ws.Cell(row, 4).Value = "Rating";
			ws.Cell(row, 5).Value = "Occupancy Rate";
			row++;

			foreach (var h in performance.HomestayPerformance)
			{
				ws.Cell(row, 1).Value = h.HomestayTitle;
				ws.Cell(row, 2).Value = h.BookingCount;
				ws.Cell(row, 3).Value = h.Revenue;
				ws.Cell(row, 4).Value = h.AverageRating;
				ws.Cell(row, 5).Value = $"{h.OccupancyRate}%";
				row++;
			}

			ws.Columns().AdjustToContents(minWidth: 12, maxWidth: 30);
		}

		// ============================================================================
		// HELPER METHODS - PDF TABLES
		// ============================================================================

		private void AddAdminOverviewPdfTable(iTextDoc document, DashboardOverviewDto overview, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("OVERVIEW STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Users", overview.TotalUsers.ToString(), normalFont);
			AddPdfDataRow(table, "Total Hosts", overview.TotalHosts.ToString(), normalFont);
			AddPdfDataRow(table, "Total Homestays", overview.TotalHomestays.ToString(), normalFont);
			AddPdfDataRow(table, "Active Homestays", overview.ActiveHomestays.ToString(), normalFont);
			AddPdfDataRow(table, "Monthly Growth", $"{overview.MonthlyGrowthRate}%", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		private void AddAdminUserStatisticsPdfTable(iTextDoc document, DashboardUserStatisticsDto stats, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("USER STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Daily Active Users", stats.DailyActiveUsers.ToString(), normalFont);
			AddPdfDataRow(table, "Monthly Active Users", stats.MonthlyActiveUsers.ToString(), normalFont);
			AddPdfDataRow(table, "Active Hosts", stats.ActiveHosts.ToString(), normalFont);
			AddPdfDataRow(table, "Conversion Rate", $"{stats.UserToHostConversionRate}%", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		private void AddAdminBookingStatisticsPdfTable(iTextDoc document, DashboardBookingStatisticsDto stats, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("BOOKING STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Bookings", stats.TotalBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Completed", stats.CompletedBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Pending", stats.PendingBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Cancelled", stats.CancelledBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Occupancy Rate", $"{stats.OccupancyRate}%", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		private void AddAdminRevenueStatisticsPdfTable(iTextDoc document, RevenueStatisticsDto stats, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("REVENUE STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Revenue", $"{stats.TotalRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Monthly Revenue", $"{stats.MonthlyRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Yearly Revenue", $"{stats.YearlyRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Refund Rate", $"{stats.RefundRate}%", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		// Host PDF Tables
		private void AddHostOverviewPdfTable(iTextDoc document, HostDashboardOverviewDto overview, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("HOST OVERVIEW")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Homestays", overview.TotalHomestays.ToString(), normalFont);
			AddPdfDataRow(table, "Active Homestays", overview.ActiveHomestays.ToString(), normalFont);
			AddPdfDataRow(table, "Total Bookings", overview.TotalBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Total Revenue", $"{overview.TotalRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Average Rating", overview.AverageRating.ToString("F1"), normalFont);
			AddPdfDataRow(table, "Monthly Revenue", $"{overview.MonthlyRevenue:N2}", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		private void AddHostRevenueStatisticsPdfTable(iTextDoc document, HostRevenueStatisticsDto revenue, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("REVENUE STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Revenue", $"{revenue.TotalRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Monthly Revenue", $"{revenue.MonthlyRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Yearly Revenue", $"{revenue.YearlyRevenue:N2}", normalFont);
			AddPdfDataRow(table, "Average Booking Value", $"{revenue.AverageBookingValue:N2}", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		private void AddHostBookingStatisticsPdfTable(iTextDoc document, HostBookingStatisticsDto bookings, PdfFont boldFont, PdfFont normalFont)
		{
			document.Add(new iTextPara("BOOKING STATISTICS")
				.SetFont(boldFont)
				.SetFontSize(12)
				.SetMarginBottom(10));

			var table = new iTextTable(new float[] { 3, 2 });
			AddPdfHeaderCell(table, "Metric", boldFont);
			AddPdfHeaderCell(table, "Value", boldFont);

			AddPdfDataRow(table, "Total Bookings", bookings.TotalBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Pending", bookings.PendingBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Confirmed", bookings.ConfirmedBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Completed", bookings.CompletedBookings.ToString(), normalFont);
			AddPdfDataRow(table, "Occupancy Rate", $"{bookings.OccupancyRate}%", normalFont);
			AddPdfDataRow(table, "Cancellation Rate", $"{bookings.CancellationRate}%", normalFont);

			document.Add(table);
			document.Add(new iTextPara(" "));
		}

		// ============================================================================
		// UTILITY METHODS
		// ============================================================================

		private void FormatExcelHeader(IXLWorksheet ws, string title)
		{
			ws.Cell(1, 1).Value = title;
			ws.Cell(1, 1).Style.Font.Bold = true;
			ws.Cell(1, 1).Style.Font.FontSize = 14;
			ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(70, 130, 180);
			ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;

			ws.Cell(2, 1).Value = "Metric";
			ws.Cell(2, 2).Value = "Value";
			ws.Row(2).Style.Font.Bold = true;
			ws.Row(2).Style.Fill.BackgroundColor = XLColor.LightGray;
		}

		private void AddExcelRow(IXLWorksheet ws, int row, string metric, object value)
		{
			ws.Cell(row, 1).Value = metric;
			ws.Cell(row, 2).Value = XLCellValue.FromObject(value);

			if (row % 2 == 0)
			{
				ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 248, 255);
			}
		}

		private void AddPdfHeaderCell(iTextTable table, string text, PdfFont font)
		{
			table.AddHeaderCell(new iTextCell()
				.Add(new iTextPara(text)
					.SetFont(font)
					.SetFontSize(10))
				.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
				.SetTextAlignment(TextAlignment.CENTER)
				.SetPadding(8));
		}

		private void AddPdfDataRow(iTextTable table, string label, string value, PdfFont font)
		{
			table.AddCell(new iTextCell()
				.Add(new iTextPara(label)
					.SetFont(font)
					.SetFontSize(9))
				.SetPadding(6));

			table.AddCell(new iTextCell()
				.Add(new iTextPara(value)
					.SetFont(font)
					.SetFontSize(9))
				.SetTextAlignment(TextAlignment.RIGHT)
				.SetPadding(6));
		}
	}
}