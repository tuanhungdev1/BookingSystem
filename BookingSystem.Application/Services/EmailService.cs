using BookingSystem.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace BookingSystem.Application.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
		{
			try
			{
				var smtpHost = _configuration["Email:SmtpHost"];
				var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
				var smtpUser = _configuration["Email:SmtpUser"];
				var smtpPass = _configuration["Email:SmtpPassword"];
				var fromEmail = _configuration["Email:FromEmail"];
				var fromName = _configuration["Email:FromName"] ?? "NextStay.vn";

				using var client = new SmtpClient(smtpHost, smtpPort);
				client.UseDefaultCredentials = false;
				client.Credentials = new NetworkCredential(smtpUser, smtpPass);
				client.EnableSsl = true;

				var mailMessage = new MailMessage
				{
					From = new MailAddress(fromEmail!, fromName),
					Subject = subject,
					Body = htmlBody,
					IsBodyHtml = true
				};

				mailMessage.To.Add(to);

				if (!string.IsNullOrEmpty(plainTextBody))
				{
					var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, "text/plain");
					mailMessage.AlternateViews.Add(plainView);
				}

				await client.SendMailAsync(mailMessage);
				_logger.LogInformation("Email sent successfully to {Email}", to);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {Email}", to);
				return false;
			}
		}

		public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink)
		{
			var subject = "Confirm Your Email - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #28a745; margin: 0;'>Welcome to NextStay.vn!</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>Please confirm your email address</h2>
                        <p>Thank you for registering with NextStay.vn. To complete your registration, please click the button below to confirm your email address:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirm Email</a>
                        </div>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{confirmationLink}</p>
                        <p><small>This link will expire in 24 hours for security reasons.</small></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>If you didn't create an account with NextStay.vn, please ignore this email.</p>
                        <p>&copy; 2025 NextStay.vn. All rights reserved.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPasswordResetAsync(string email, string resetLink)
		{
			var subject = "Reset Your Password - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #dc3545; margin: 0;'>Password Reset Request</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>Reset Your Password</h2>
                        <p>We received a request to reset your password for your NextStay.vn account. Click the button below to reset it:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                        </div>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
                        <p><small>This link will expire in 1 hour for security reasons.</small></p>
                        <p><strong>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</strong></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. All rights reserved.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> Send2FACodeAsync(string email, string code)
		{
			var subject = "Your 2FA Code - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #17a2b8; margin: 0;'>Two-Factor Authentication</h1>
                    </div>
                    <div style='padding: 30px 20px; text-align: center;'>
                        <h2>Your Verification Code</h2>
                        <p>Enter this code to complete your login:</p>
                        <div style='font-size: 32px; font-weight: bold; color: #17a2b8; letter-spacing: 5px; margin: 30px 0; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>
                            {code}
                        </div>
                        <p><small>This code will expire in 5 minutes.</small></p>
                        <p><small>If you didn't request this code, please contact support immediately.</small></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. All rights reserved.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
		{
			var subject = "Welcome to NextStay.vn!";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Welcome to NextStay.vn, {firstName}!</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>You're all set!</h2>
                        <p>Thank you for joining NextStay.vn. Your email has been confirmed and your account is now active.</p>
                        <p>Here's what you can do now:</p>
                        <ul>
                            <li>Browse hotels and accommodations in your area</li>
                            <li>Book your first stay</li>
                            <li>Save your favorite destinations</li>
                            <li>Manage your NextStays in real-time</li>
                        </ul>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://NextStay.vn/hotels' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Start NextStay</a>
                        </div>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. All rights reserved.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingConfirmationAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkIn, DateTime checkOut, decimal totalAmount)
		{
			var subject = "Booking Confirmation - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Booking Confirmed!</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>Your booking has been confirmed. Here are the details:</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Booking Code:</strong> {bookingCode}</p>
					<p><strong>Homestay:</strong> {homestayName}</p>
					<p><strong>Check-in:</strong> {checkIn:dddd, dd MMMM yyyy}</p>
					<p><strong>Check-out:</strong> {checkOut:dddd, dd MMMM yyyy}</p>
					<p><strong>Total Amount:</strong> {totalAmount:N0} VND</p>
				</div>
				<p>We look forward to hosting you!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingRejectedAsync(string email, string guestName, string bookingCode, string homestayName, string reason)
		{
			var subject = "Booking Rejected - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #dc3545; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Booking Rejected</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>Unfortunately, your booking <strong>{bookingCode}</strong> for <strong>{homestayName}</strong> has been rejected.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Reason:</strong> {reason}</p>
				</div>
				<p>Any payment made will be refunded within 5-7 business days.</p>
				<p>Please feel free to search for other properties on NextStay.vn</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingCancelledAsync(string email, string guestName, string bookingCode, string homestayName)
		{
			var subject = "Booking Cancelled - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #ffc107; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Booking Cancelled</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>Your booking <strong>{bookingCode}</strong> for <strong>{homestayName}</strong> has been cancelled.</p>
				<p>If you made a payment, a refund will be processed according to the cancellation policy.</p>
				<p>We hope to serve you again in the future.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPaymentSuccessAsync(string email, string guestName, string bookingCode, decimal amount, string paymentMethod)
		{
			var subject = "Payment Successful - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Payment Successful</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>We have successfully received your payment.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Booking Code:</strong> {bookingCode}</p>
					<p><strong>Amount Paid:</strong> {amount:N0} VND</p>
					<p><strong>Payment Method:</strong> {paymentMethod}</p>
					<p><strong>Date:</strong> {DateTime.UtcNow:dd MMMM yyyy, HH:mm}</p>
				</div>
				<p>Thank you for your payment!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPaymentFailedAsync(string email, string guestName, string bookingCode, string reason)
		{
			var subject = "Payment Failed - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #dc3545; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Payment Failed</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>Unfortunately, your payment for booking <strong>{bookingCode}</strong> has failed.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Reason:</strong> {reason}</p>
				</div>
				<p>Please try again or contact support if the issue persists.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendRefundProcessedAsync(string email, string guestName, string bookingCode, decimal refundAmount)
		{
			var subject = "Refund Processed - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #17a2b8; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Refund Processed</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>A refund has been processed for your booking <strong>{bookingCode}</strong>.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Refund Amount:</strong> {refundAmount:N0} VND</p>
					<p><strong>Processing Date:</strong> {DateTime.UtcNow:dd MMMM yyyy}</p>
				</div>
				<p>The refund will be credited to your original payment method within 5-7 business days.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendCheckInReminderAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkInDate, string checkInTime)
		{
			var subject = "Check-in Reminder - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #17a2b8; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Check-in Reminder</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {guestName},</p>
				<p>This is a reminder that your check-in is coming soon!</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Booking Code:</strong> {bookingCode}</p>
					<p><strong>Homestay:</strong> {homestayName}</p>
					<p><strong>Check-in Date:</strong> {checkInDate:dddd, dd MMMM yyyy}</p>
					<p><strong>Check-in Time:</strong> {checkInTime}</p>
				</div>
				<p>Please arrive on time and bring your booking confirmation.</p>
				<p>Have a wonderful stay!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendNewBookingNotificationToHostAsync(string hostEmail, string hostName, string bookingCode, string guestName, string homestayName, DateTime checkIn, DateTime checkOut)
		{
			var subject = "New Booking Received - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #007bff; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>New Booking Received!</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Dear {hostName},</p>
				<p>You have received a new booking for your property.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Booking Code:</strong> {bookingCode}</p>
					<p><strong>Guest Name:</strong> {guestName}</p>
					<p><strong>Property:</strong> {homestayName}</p>
					<p><strong>Check-in:</strong> {checkIn:dddd, dd MMMM yyyy}</p>
					<p><strong>Check-out:</strong> {checkOut:dddd, dd MMMM yyyy}</p>
				</div>
				<p>Please log in to your host dashboard to review and confirm this booking.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. All rights reserved.</p>
			</div>
		</div>";

			return await SendEmailAsync(hostEmail, subject, htmlBody);
		}
	}
}
