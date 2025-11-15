namespace BookingSystem.Application.Contracts
{
	public interface IEmailService
	{
		Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
		Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink);
		Task<bool> SendPasswordResetAsync(string email, string resetLink);
		Task<bool> Send2FACodeAsync(string email, string code);
		Task<bool> SendWelcomeEmailAsync(string email, string firstName);

		Task<bool> SendBookingConfirmationAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkIn, DateTime checkOut, decimal totalAmount);
		Task<bool> SendBookingRejectedAsync(string email, string guestName, string bookingCode, string homestayName, string reason);
		Task<bool> SendBookingCancelledAsync(string email, string guestName, string bookingCode, string homestayName);
		Task<bool> SendPaymentSuccessAsync(string email, string guestName, string bookingCode, decimal amount, string paymentMethod);
		Task<bool> SendPaymentFailedAsync(string email, string guestName, string bookingCode, string reason);
		Task<bool> SendRefundProcessedAsync(string email, string guestName, string bookingCode, decimal refundAmount);
		Task<bool> SendCheckInReminderAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkInDate, string checkInTime);
		Task<bool> SendNewBookingNotificationToHostAsync(string hostEmail, string hostName, string bookingCode, string guestName, string homestayName, DateTime checkIn, DateTime checkOut);
	}

}
