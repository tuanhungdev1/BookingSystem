namespace BookingSystem.Application.Contracts
{
	public interface IEmailService
	{
		Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
		Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink);
		Task<bool> SendPasswordResetAsync(string email, string resetLink);
		Task<bool> Send2FACodeAsync(string email, string code);
		Task<bool> SendWelcomeEmailAsync(string email, string firstName);
	}
}
