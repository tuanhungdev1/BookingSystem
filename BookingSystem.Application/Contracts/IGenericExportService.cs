using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
	public interface IGenericExportService
	{
		/// <summary>
		/// Export dữ liệu generic sang Excel
		/// </summary>
		Task<byte[]> ExportToExcelAsync<T>(
			IEnumerable<T> data,
			string sheetName = "Sheet1",
			string fileName = "export.xlsx") where T : class;

		/// <summary>
		/// Export dữ liệu generic sang PDF
		/// </summary>
		Task<byte[]> ExportToPdfAsync<T>(
			IEnumerable<T> data,
			string title = "Report",
			string fileName = "export.pdf") where T : class;

		/// <summary>
		/// Export dữ liệu generic sang CSV
		/// </summary>
		Task<byte[]> ExportToCSVAsync<T>(
			IEnumerable<T> data,
			string fileName = "export.csv") where T : class;
	}
}
