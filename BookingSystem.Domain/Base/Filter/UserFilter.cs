using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Base.Filter
{
    public class UserFilter : PaginationFilter
	{
		// Text Search
		public string? Search { get; set; }
		// Sorting
		[RegularExpression("^(userName|email|fullName|createdAt|lastLoginAt)?$", ErrorMessage = "SortBy must be one of: userName, email, fullName, createdAt, lastLoginAt")]
		public string? SortBy { get; set; } = "userName"; // Default to "userName" or adjust as needed

		[RegularExpression("^(asc|desc)?$", ErrorMessage = "SortOrder must be 'asc' or 'desc'")]
		public string? SortOrder { get; set; } = "asc";

		// Status Filters - Use string to handle "all", true/false
		[RegularExpression("^(all|true|false)?$", ErrorMessage = "IsActive must be 'all', 'true', or 'false'")]
		public string? IsActive { get; set; } // "all" means no filter

		[RegularExpression("^(all|true|false)?$", ErrorMessage = "IsLocked must be 'all', 'true', or 'false'")]
		public string? IsLocked { get; set; } // "all" means no filter

		[RegularExpression("^(all|true|false)?$", ErrorMessage = "IsEmailConfirmed must be 'all', 'true', or 'false'")]
		public string? IsEmailConfirmed { get; set; } // "all" means no filter

		// Role Filter
		public string[]? Roles { get; set; } // Or List<string> if preferred

		// Date Range - Use string for ISO date, or DateTime? if parsing in controller
		public string? CreatedAtFrom { get; set; } // ISO format: "YYYY-MM-DDTHH:mm:ssZ"
		public string? CreatedAtTo { get; set; }
	}
}
