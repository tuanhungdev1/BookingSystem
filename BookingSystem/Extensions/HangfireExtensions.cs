using BookingSystem.Application.Contracts;
using Hangfire;
using Hangfire.States;

namespace BookingSystem.Extensions
{
	public static class HangfireExtensions
	{
		public static IServiceCollection AddHangfireConfiguration(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddHangfire(config =>
				config.UseSqlServerStorage(
					configuration.GetConnectionString("ApplicationDB"),
					new Hangfire.SqlServer.SqlServerStorageOptions
					{
						CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
						SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
						QueuePollInterval = TimeSpan.Zero,
						UseRecommendedIsolationLevel = true,
						DisableGlobalLocks = true
					}
				)
			);

			services.AddHangfireServer();

			return services;
		}

		/// <summary>
		/// Cấu hình các Recurring Job
		/// </summary>
		public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
		{
			// Hiển thị Hangfire Dashboard
			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				// Có thể thêm authorization nếu cần
				// Authorization = new[] { new HangfireAuthorizationFilter() }
			});

			// Đăng ký các Recurring Job
			RegisterRecurringJobs();

			return app;
		}

		/// <summary>
		/// Đăng ký tất cả các Recurring Job
		/// </summary>
		private static void RegisterRecurringJobs()
		{
			// Job: Xử lý booking hết hạn thanh toán mỗi 5 phút
			RecurringJob.AddOrUpdate<IBookingService>(
				recurringJobId: "process-expired-bookings",
				methodCall: service => service.ProcessExpiredPendingBookingsAsync(),
				cronExpression: Cron.MinuteInterval(5),
				timeZone: TimeZoneInfo.Local,        
				queue: EnqueuedState.DefaultQueue
			);

			// Có thể thêm các job khác ở đây trong tương lai
			// RecurringJob.AddOrUpdate<IOtherService>(
			//     "other-job-name",
			//     service => service.SomeMethodAsync(),
			//     Cron.Daily(10, 30),
			//     TimeZone.CurrentTimeZone
			// );
		}
	}
}
