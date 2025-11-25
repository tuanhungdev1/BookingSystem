namespace BookingSystem.Extensions
{
	public static class StringExtensions
	{
		public static bool IsValidExportFormat(this string format)
		{
			return format.ToLower() is "excel" or "pdf" or "csv";
		}
	}
}
