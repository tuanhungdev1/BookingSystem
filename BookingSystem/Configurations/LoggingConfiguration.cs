using Serilog.Events;
using Serilog;

namespace BookingSystem.Configurations
{
	public static class LoggingConfiguration
	{
		public static void ConfigureSerilog(this WebApplicationBuilder builder)
		{
			// Bootstrap logger
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateBootstrapLogger();

			// Configure Serilog with settings
			builder.Host.UseSerilog((context, services, configuration) => configuration
				.ReadFrom.Configuration(context.Configuration)
				.ReadFrom.Services(services)
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithProperty("Application", context.Configuration["ApplicationName"] ?? "MyWebAPI")
			);
		}
	}
}
