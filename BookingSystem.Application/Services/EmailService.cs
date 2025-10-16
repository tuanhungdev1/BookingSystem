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
	}
}
