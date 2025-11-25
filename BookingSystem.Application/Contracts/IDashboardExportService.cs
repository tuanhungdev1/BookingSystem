using BookingSystem.Application.DTOs.DashboardDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
	public interface IDashboardExportService
	{
		/// <summary>
		/// Export Admin Dashboard sang Excel
		/// </summary>
		Task<byte[]> ExportAdminDashboardToExcelAsync(
			CompleteDashboardDto dashboard,
			int months = 12);

		/// <summary>
		/// Export Admin Dashboard sang PDF
		/// </summary>
		Task<byte[]> ExportAdminDashboardToPdfAsync(
			CompleteDashboardDto dashboard,
			int months = 12);

		/// <summary>
		/// Export Admin Dashboard sang CSV
		/// </summary>
		Task<byte[]> ExportAdminDashboardToCSVAsync(
			CompleteDashboardDto dashboard,
			int months = 12);

		/// <summary>
		/// Export Host Dashboard sang Excel
		/// </summary>
		Task<byte[]> ExportHostDashboardToExcelAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12);

		/// <summary>
		/// Export Host Dashboard sang PDF
		/// </summary>
		Task<byte[]> ExportHostDashboardToPdfAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12);

		/// <summary>
		/// Export Host Dashboard sang CSV
		/// </summary>
		Task<byte[]> ExportHostDashboardToCSVAsync(
			CompleteHostDashboardDto dashboard,
			int months = 12);
	}
}
