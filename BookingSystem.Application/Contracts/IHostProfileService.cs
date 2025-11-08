using BookingSystem.Application.DTOs.HostProfileDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Contracts
{
    public interface IHostProfileService
    {
		Task<HostProfileDto?> GetHostProfileByIdAsync(int userId, int currentUserId);
		Task<PagedResult<HostProfileDto>> GetAllHostProfileAsync(HostProfileFilter hostProfileFilter);
		Task<bool> RegisterHostAsync(CreateHostProfileDto dto);
		Task<bool> UpdateHostProfileAsync(int hostProfileId, int currentUserId, UpdateHostProfileDto dto);
		Task<bool> RemoveHostProfileAsync(int hostId, int currentUserId);
		Task<bool> ApproveHostProfileAsync(int hostProfileId, int adminId, string? note = null);
		Task<bool> RejectHostProfileAsync(int hostProfileId, int adminId, string reason);
		Task<bool> ReviewHostProfileAsync(int id, int adminId, string status, string? note);
		Task<bool> MarkAsSuperhostAsync(int hostProfileId, int adminId, bool isSuperhost);
		Task<bool> ToggleActiveStatusAsync(int hostProfileId, int adminId, bool isActive);
		Task UpdateStatisticsAsync(int hostProfileId, int totalHomestays, int totalBookings, decimal averageRating, int responseRate, TimeSpan? avgResponseTime);
		// Document Upload
		Task<string> UploadIdentityCardAsync(int hostId, UploadIdentityCardDto dto);
		Task<string> UploadBusinessLicenseAsync(int hostId, UploadBusinessLicenseDto licenseDto);
		Task<string> UploadTaxCodeDocumentAsync(int hostId, UploadTaxCodeDocumentDto documentDto);
	}
}
