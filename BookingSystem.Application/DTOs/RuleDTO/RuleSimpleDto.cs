namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class RuleSimpleDto
	{
		public int Id { get; set; }
		public string RuleName { get; set; } = string.Empty;
		public string? RuleDescription { get; set; }
		public string? IconUrl { get; set; }
		public string RuleType { get; set; } = string.Empty;
		public int DisplayOrder { get; set; }

		// Thông tin từ bảng trung gian (nếu cần)
		public string? CustomNote { get; set; }
	}
}
