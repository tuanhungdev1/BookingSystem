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
			var subject = "Xác nhận email - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #28a745; margin: 0;'>Chào mừng đến với NextStay.vn!</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>Vui lòng xác nhận địa chỉ email của bạn</h2>
                        <p>Cảm ơn bạn đã đăng ký tại NextStay.vn. Để hoàn tất đăng ký, vui lòng bấm nút bên dưới để xác nhận email:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Xác nhận email</a>
                        </div>
                        <p>Nếu nút không hoạt động, hãy sao chép và dán đường dẫn sau vào trình duyệt của bạn:</p>
                        <p style='word-break: break-all; color: #007bff;'>{confirmationLink}</p>
                        <p><small>Đường dẫn này sẽ hết hạn sau 24 giờ vì lý do bảo mật.</small></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>Nếu bạn không tạo tài khoản tại NextStay.vn, vui lòng bỏ qua email này.</p>
                        <p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPasswordResetAsync(string email, string resetLink)
		{
			var subject = "Đặt lại mật khẩu - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #dc3545; margin: 0;'>Yêu cầu đặt lại mật khẩu</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>Đặt lại mật khẩu</h2>
                        <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản NextStay.vn của bạn. Bấm nút bên dưới để đặt lại mật khẩu:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Đặt lại mật khẩu</a>
                        </div>
                        <p>Nếu nút không hoạt động, hãy sao chép và dán đường dẫn sau vào trình duyệt của bạn:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
                        <p><small>Liên kết này sẽ hết hạn sau 1 giờ vì lý do bảo mật.</small></p>
                        <p><strong>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này — mật khẩu của bạn sẽ không thay đổi.</strong></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> Send2FACodeAsync(string email, string code)
		{
			var subject = "Mã 2FA - NextStay.vn";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #17a2b8; margin: 0;'>Xác thực hai yếu tố</h1>
                    </div>
                    <div style='padding: 30px 20px; text-align: center;'>
                        <h2>Mã xác minh của bạn</h2>
                        <p>Nhập mã này để hoàn tất đăng nhập:</p>
                        <div style='font-size: 32px; font-weight: bold; color: #17a2b8; letter-spacing: 5px; margin: 30px 0; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>
                            {code}
                        </div>
                        <p><small>Mã này sẽ hết hạn sau 5 phút.</small></p>
                        <p><small>Nếu bạn không yêu cầu mã này, vui lòng liên hệ bộ phận hỗ trợ ngay lập tức.</small></p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
		{
			var subject = "Chào mừng đến với NextStay.vn!";
			var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Chào mừng {firstName} đến với NextStay.vn!</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2>Bạn đã sẵn sàng!</h2>
                        <p>Cảm ơn bạn đã gia nhập NextStay.vn. Email của bạn đã được xác nhận và tài khoản của bạn hiện đã hoạt động.</p>
                        <p>Bạn có thể làm những việc sau:</p>
                        <ul>
                            <li>Duyệt khách sạn và nơi lưu trú trong khu vực</li>
                            <li>Đặt chỗ lần đầu tiên</li>
                            <li>Lưu địa điểm yêu thích</li>
                            <li>Quản lý các NextStay của bạn theo thời gian thực</li>
                        </ul>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://NextStay.vn/hotels' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Bắt đầu với NextStay</a>
                        </div>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
                    </div>
                </div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingConfirmationAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkIn, DateTime checkOut, decimal totalAmount)
		{
			var subject = "Xác nhận đặt phòng - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Đặt phòng đã được xác nhận!</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Đặt phòng của bạn đã được xác nhận. Chi tiết như sau:</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Mã đặt phòng:</strong> {bookingCode}</p>
					<p><strong>Tên nhà/phòng:</strong> {homestayName}</p>
					<p><strong>Nhận phòng:</strong> {checkIn:dddd, dd MMMM yyyy}</p>
					<p><strong>Trả phòng:</strong> {checkOut:dddd, dd MMMM yyyy}</p>
					<p><strong>Tổng tiền:</strong> {totalAmount:N0} VND</p>
				</div>
				<p>Chúng tôi mong được đón tiếp bạn!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingRejectedAsync(string email, string guestName, string bookingCode, string homestayName, string reason)
		{
			var subject = "Đặt phòng bị từ chối - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #dc3545; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Đặt phòng bị từ chối</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Rất tiếc, đặt phòng <strong>{bookingCode}</strong> cho <strong>{homestayName}</strong> đã bị từ chối.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Lý do:</strong> {reason}</p>
				</div>
				<p>Mọi khoản thanh toán (nếu có) sẽ được hoàn trong vòng 5-7 ngày làm việc.</p>
				<p>Vui lòng tìm các chỗ ở khác trên NextStay.vn.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendBookingCancelledAsync(string email, string guestName, string bookingCode, string homestayName)
		{
			var subject = "Đặt phòng đã hủy - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #ffc107; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Đặt phòng đã hủy</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Đặt phòng <strong>{bookingCode}</strong> cho <strong>{homestayName}</strong> của bạn đã bị hủy.</p>
				<p>Nếu bạn đã thanh toán, khoản hoàn sẽ được xử lý theo chính sách hủy.</p>
				<p>Rất mong được phục vụ bạn trong tương lai.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPaymentSuccessAsync(string email, string guestName, string bookingCode, decimal amount, string paymentMethod)
		{
			var subject = "Thanh toán thành công - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #28a745; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Thanh toán thành công</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Chúng tôi đã nhận được thanh toán của bạn.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Mã đặt phòng:</strong> {bookingCode}</p>
					<p><strong>Số tiền đã thanh toán:</strong> {amount:N0} VND</p>
					<p><strong>Phương thức thanh toán:</strong> {paymentMethod}</p>
					<p><strong>Thời gian:</strong> {DateTime.UtcNow:dd MMMM yyyy, HH:mm}</p>
				</div>
				<p>Cảm ơn bạn đã thanh toán!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendPaymentFailedAsync(string email, string guestName, string bookingCode, string reason)
		{
			var subject = "Thanh toán thất bại - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #dc3545; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Thanh toán thất bại</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Rất tiếc, thanh toán cho đặt phòng <strong>{bookingCode}</strong> không thành công.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Lý do:</strong> {reason}</p>
				</div>
				<p>Vui lòng thử lại hoặc liên hệ bộ phận hỗ trợ nếu sự cố tiếp tục xảy ra.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendRefundProcessedAsync(string email, string guestName, string bookingCode, decimal refundAmount)
		{
			var subject = "Hoàn tiền đã được xử lý - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #17a2b8; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Hoàn tiền đã được xử lý</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Một khoản hoàn tiền đã được xử lý cho đặt phòng <strong>{bookingCode}</strong> của bạn.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Số tiền hoàn:</strong> {refundAmount:N0} VND</p>
					<p><strong>Ngày xử lý:</strong> {DateTime.UtcNow:dd MMMM yyyy}</p>
				</div>
				<p>Số tiền hoàn sẽ được trả về phương thức thanh toán ban đầu trong vòng 5-7 ngày làm việc.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendCheckInReminderAsync(string email, string guestName, string bookingCode, string homestayName, DateTime checkInDate, string checkInTime)
		{
			var subject = "Nhắc nhở nhận phòng - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #17a2b8; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Nhắc nhở nhận phòng</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {guestName},</p>
				<p>Đây là lời nhắc: thời gian nhận phòng sắp đến!</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Mã đặt phòng:</strong> {bookingCode}</p>
					<p><strong>Tên nhà/phòng:</strong> {homestayName}</p>
					<p><strong>Ngày nhận phòng:</strong> {checkInDate:dddd, dd MMMM yyyy}</p>
					<p><strong>Thời gian nhận phòng:</strong> {checkInTime}</p>
				</div>
				<p>Vui lòng đến đúng giờ và mang theo xác nhận đặt phòng.</p>
				<p>Chúc bạn có một kỳ nghỉ tuyệt vời!</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(email, subject, htmlBody);
		}

		public async Task<bool> SendNewBookingNotificationToHostAsync(string hostEmail, string hostName, string bookingCode, string guestName, string homestayName, DateTime checkIn, DateTime checkOut)
		{
			var subject = "Có đặt phòng mới - NextStay.vn";
			var htmlBody = $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
			<div style='background-color: #007bff; padding: 30px 20px; text-align: center;'>
				<h1 style='color: white; margin: 0;'>Có đặt phòng mới!</h1>
			</div>
			<div style='padding: 30px 20px;'>
				<p>Kính gửi {hostName},</p>
				<p>Bạn vừa nhận được một đặt phòng mới cho tài sản của mình.</p>
				<div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
					<p><strong>Mã đặt phòng:</strong> {bookingCode}</p>
					<p><strong>Tên khách:</strong> {guestName}</p>
					<p><strong>Tài sản:</strong> {homestayName}</p>
					<p><strong>Nhận phòng:</strong> {checkIn:dddd, dd MMMM yyyy}</p>
					<p><strong>Trả phòng:</strong> {checkOut:dddd, dd MMMM yyyy}</p>
				</div>
				<p>Vui lòng đăng nhập vào bảng điều khiển chủ nhà để xem và xác nhận đặt phòng này.</p>
			</div>
			<div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
				<p>&copy; 2025 NextStay.vn. Bảo lưu mọi quyền.</p>
			</div>
		</div>";

			return await SendEmailAsync(hostEmail, subject, htmlBody);
		}
	}
}
