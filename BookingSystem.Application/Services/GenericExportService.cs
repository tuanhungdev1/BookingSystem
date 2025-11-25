using BookingSystem.Application.Contracts;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Geom;
using iText.Kernel.Colors;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

using iTextDoc = iText.Layout.Document;
using iTextPara = iText.Layout.Element.Paragraph;
using iTextTable = iText.Layout.Element.Table;
using iTextCell = iText.Layout.Element.Cell;

namespace BookingSystem.Application.Services
{
	public class GenericExportService : IGenericExportService
	{
		private readonly ILogger<GenericExportService> _logger;

		public GenericExportService(ILogger<GenericExportService> logger)
		{
			_logger = logger;
		}

		// ============================================================================
		// EXPORT TO EXCEL - GENERIC
		// ============================================================================
		public async Task<byte[]> ExportToExcelAsync<T>(
			IEnumerable<T> data,
			string sheetName = "Sheet1",
			string fileName = "export.xlsx") where T : class
		{
			_logger.LogInformation("Exporting {Count} {Type} records to Excel", data.Count(), typeof(T).Name);

			try
			{
				using (var workbook = new XLWorkbook())
				{
					var worksheet = workbook.Worksheets.Add(sheetName);

					var properties = GetExportableProperties<T>();

					if (!properties.Any())
						throw new Exception("Không tìm thấy property nào để export");

					worksheet.Columns().Width = 15;

					// Header row
					for (int col = 1; col <= properties.Count; col++)
					{
						var headerCell = worksheet.Cell(1, col);
						headerCell.Value = properties[col - 1].Name;

						var propertyName = properties[col - 1].Name;
						var columnWidth = Math.Max(propertyName.Length + 2, 12);
						worksheet.Column(col).Width = columnWidth;
					}

					// Header formatting
					var headerRow = worksheet.Row(1);
					headerRow.Style.Font.Bold = true;
					headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(70, 130, 180);
					headerRow.Style.Font.FontColor = XLColor.White;
					headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
					headerRow.Height = 20;

					worksheet.SheetView.FreezeRows(1);

					// Data rows
					int row = 2;
					foreach (var item in data)
					{
						for (int col = 1; col <= properties.Count; col++)
						{
							var property = properties[col - 1];
							var value = property.GetValue(item);

							var cell = worksheet.Cell(row, col);

							if (value is decimal dec)
							{
								cell.Value = dec;
								cell.Style.NumberFormat.Format = "#,##0.00";
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
							}
							else if (value is int intVal)
							{
								cell.Value = intVal;
								cell.Style.NumberFormat.Format = "0";
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
							}
							else if (value is DateTime dt)
							{
								cell.Value = dt;
								cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
							}
							else if (value is bool b)
							{
								cell.Value = b ? "Có" : "Không";
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
							}
							else if (value is Enum en)
							{
								cell.Value = GetEnumDisplayValue(en);
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
							}
							else
							{
								cell.Value = value?.ToString() ?? "";
								cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
							}

							if (value?.ToString()?.Length > 50)
							{
								cell.Style.Alignment.WrapText = true;
							}
						}

						row++;
					}

					worksheet.Columns().AdjustToContents(minWidth: 10, maxWidth: 50);

					// Alternating row colors
					for (int i = 2; i <= row - 1; i++)
					{
						if (i % 2 == 0)
						{
							worksheet.Row(i).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 248, 255);
						}
					}

					using (var memoryStream = new MemoryStream())
					{
						workbook.SaveAs(memoryStream);
						return memoryStream.ToArray();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting to Excel for type {Type}", typeof(T).Name);
				throw new Exception($"Failed to export {typeof(T).Name} to Excel", ex);
			}
		}

		// ============================================================================
		// EXPORT TO PDF - GENERIC (CẢI THIỆN: Hỗ trợ Landscape)
		// ============================================================================
		public async Task<byte[]> ExportToPdfAsync<T>(
			IEnumerable<T> data,
			string title = "Báo cáo",
			string fileName = "export.pdf") where T : class
		{
			await Task.Yield();

			try
			{
				using var stream = new MemoryStream();
				using var writer = new PdfWriter(stream);

				// ✅ THÊM: Tự động chọn PageSize dựa trên số cột
				var properties = GetExportableProperties<T>();
				var pageSize = DeterminePageSize(properties.Count);

				using var pdf = new PdfDocument(writer);
				pdf.SetDefaultPageSize(pageSize);

				var document = new iTextDoc(pdf);
				document.SetMargins(30, 20, 30, 20);

				// Font
				var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
				var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

				// Tiêu đề
				document.Add(new iTextPara(title)
					.SetFont(boldFont)
					.SetFontSize(18)
					.SetTextAlignment(TextAlignment.CENTER)
					.SetMarginBottom(10));

				document.Add(new iTextPara($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
					.SetFont(normalFont)
					.SetFontSize(10)
					.SetTextAlignment(TextAlignment.RIGHT)
					.SetMarginBottom(20));

				if (!properties.Any() || !data.Any())
				{
					document.Add(new iTextPara("Không có dữ liệu để xuất.")
						.SetFont(normalFont)
						.SetFontSize(12));
					document.Close();
					return stream.ToArray();
				}

				// ✅ CẢI THIỆN: Tính độ rộng cột tự động cho cả dọc và ngang
				var columnWidths = CalculateColumnWidths(pageSize, properties.Count);

				var table = new iTextTable(columnWidths);
				table.SetWidth(UnitValue.CreatePercentValue(100));

				// Header
				foreach (var prop in properties)
				{
					table.AddHeaderCell(new iTextCell()
						.Add(new iTextPara(prop.Name)
							.SetFont(boldFont)
							.SetFontSize(9))
						.SetBackgroundColor(ColorConstants.LIGHT_GRAY)
						.SetTextAlignment(TextAlignment.CENTER)
						.SetPadding(6));
				}

				// Data rows
				int rowCount = 0;
				foreach (var item in data)
				{
					foreach (var prop in properties)
					{
						var value = prop.GetValue(item);
						string text;
						TextAlignment alignment = TextAlignment.LEFT;

						if (value is decimal dec)
						{
							text = dec.ToString("N2");
							alignment = TextAlignment.RIGHT;
						}
						else if (value is int intVal)
						{
							text = intVal.ToString();
							alignment = TextAlignment.CENTER;
						}
						else if (value is DateTime dt)
						{
							text = dt.ToString("dd/MM/yyyy HH:mm:ss");
							alignment = TextAlignment.CENTER;
						}
						else if (value is bool b)
						{
							text = b ? "Có" : "Không";
							alignment = TextAlignment.CENTER;
						}
						else if (value is Enum en)
						{
							text = GetEnumDisplayValue(en);
							alignment = TextAlignment.CENTER;
						}
						else
						{
							text = value?.ToString() ?? "";
						}

						var cell = new iTextCell()
							.Add(new iTextPara(text)
								.SetFont(normalFont)
								.SetFontSize(8))
							.SetTextAlignment(alignment)
							.SetPadding(5);

						if (rowCount % 2 == 0)
						{
							cell.SetBackgroundColor(new DeviceRgb(240, 248, 255));
						}

						table.AddCell(cell);
					}

					rowCount++;
				}

				document.Add(table);
				document.Close();
				return stream.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Export PDF failed");
				throw new ApplicationException("Không thể xuất file PDF", ex);
			}
		}

		// ============================================================================
		// EXPORT TO CSV - GENERIC
		// ============================================================================
		public async Task<byte[]> ExportToCSVAsync<T>(
			IEnumerable<T> data,
			string fileName = "export.csv") where T : class
		{
			_logger.LogInformation("Exporting {Count} {Type} records to CSV", data.Count(), typeof(T).Name);

			try
			{
				var sb = new StringBuilder();
				var properties = GetExportableProperties<T>();

				if (!properties.Any())
					throw new Exception("Không tìm thấy property nào để export");

				sb.Append('\ufeff'); // BOM for UTF-8

				// CSV Header
				var headers = string.Join(",", properties.Select(p => EscapeCSVValue(p.Name)));
				sb.AppendLine(headers);

				// CSV Data
				foreach (var item in data)
				{
					var values = new List<string>();
					foreach (var property in properties)
					{
						var value = property.GetValue(item);
						string formattedValue;

						if (value is decimal dec)
						{
							formattedValue = dec.ToString("N2");
						}
						else if (value is int intVal)
						{
							formattedValue = intVal.ToString();
						}
						else if (value is DateTime dt)
						{
							formattedValue = dt.ToString("yyyy-MM-dd HH:mm:ss");
						}
						else if (value is bool b)
						{
							formattedValue = b ? "Có" : "Không";
						}
						else if (value is Enum en)
						{
							formattedValue = GetEnumDisplayValue(en);
						}
						else
						{
							formattedValue = value?.ToString() ?? "";
						}

						values.Add(EscapeCSVValue(formattedValue));
					}

					sb.AppendLine(string.Join(",", values));
				}

				return Encoding.UTF8.GetBytes(sb.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting to CSV for type {Type}", typeof(T).Name);
				throw new Exception($"Failed to export {typeof(T).Name} to CSV", ex);
			}
		}

		// ============================================================================
		// HELPER METHODS (THÊM: Xác định PageSize & tính Column Width)
		// ============================================================================

		/// <summary>
		/// ✅ THÊM: Xác định PageSize (Portrait/Landscape) dựa trên số cột
		/// </summary>
		private PageSize DeterminePageSize(int columnCount)
		{
			// Nếu có nhiều cột, sử dụng Landscape (A4 ngang)
			if (columnCount > 8)
			{
				_logger.LogInformation("Using Landscape page size for {ColumnCount} columns", columnCount);
				return PageSize.A4.Rotate(); // A4 Landscape (297mm x 210mm)
			}

			// Nếu cột ít, sử dụng Portrait (A4 dọc)
			return PageSize.A4; // A4 Portrait (210mm x 297mm)
		}

		/// <summary>
		/// ✅ THÊM: Tính độ rộng cột tối ưu cho page size
		/// </summary>
		private float[] CalculateColumnWidths(PageSize pageSize, int columnCount)
		{
			// Tính chiều rộng có sẵn (trừ margin)
			float pageWidth = pageSize.GetWidth() - 50; // 50 = margin trái + phải

			// Chia đều cho từng cột
			float columnWidth = pageWidth / columnCount;

			// Đảm bảo độ rộng tối thiểu
			columnWidth = Math.Max(columnWidth, 40);

			// Tạo array độ rộng cột
			var widths = new float[columnCount];
			for (int i = 0; i < columnCount; i++)
			{
				widths[i] = columnWidth;
			}

			return widths;
		}

		private List<PropertyInfo> GetExportableProperties<T>() where T : class
		{
			return typeof(T)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p =>
					p.PropertyType.IsValueType ||
					p.PropertyType == typeof(string) ||
					p.PropertyType == typeof(DateTime) ||
					p.PropertyType == typeof(DateTime?) ||
					p.PropertyType == typeof(decimal) ||
					p.PropertyType == typeof(decimal?) ||
					p.PropertyType == typeof(int) ||
					p.PropertyType == typeof(int?) ||
					p.PropertyType == typeof(bool) ||
					p.PropertyType == typeof(bool?) ||
					p.PropertyType.IsEnum
				)
				.ToList();
		}

		private string GetEnumDisplayValue(Enum? value)
		{
			if (value == null)
				return "";

			return value.ToString() switch
			{
				// Booking Status
				"Pending" => "Chờ xác nhận",
				"Confirmed" => "Đã xác nhận",
				"CheckedIn" => "Đã check-in",
				"CheckedOut" => "Đã check-out",
				"Completed" => "Hoàn thành",
				"Cancelled" => "Đã hủy",
				"Rejected" => "Từ chối",
				"NoShow" => "Không đến",

				// Payment Status
				"Processing" => "Đang xử lý",
				"Failed" => "Thất bại",
				"Refunded" => "Đã hoàn tiền",
				"PartiallyRefunded" => "Hoàn tiền một phần",

				// Payment Method
				"VNPay" => "VNPay",
				"ZaloPay" => "ZaloPay",
				"Momo" => "Momo",
				"BankTransfer" => "Chuyển khoản",
				"Cash" => "Tiền mặt",

				// Gender
				"Male" => "Nam",
				"Female" => "Nữ",
				"Other" => "Khác",

				_ => value.ToString()
			};
		}

		private string EscapeCSVValue(string? value)
		{
			if (string.IsNullOrEmpty(value))
				return "\"\"";

			if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
			{
				var escaped = value.Replace("\"", "\"\"");
				return $"\"{escaped}\"";
			}

			return $"\"{value}\"";
		}
	}
}