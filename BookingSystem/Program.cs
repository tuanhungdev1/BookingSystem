using BookingSystem.Application.DI;
using BookingSystem.Application.Mappings;
using BookingSystem.Extensions;
using BookingSystem.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));
// Add services to the container.
builder.Services.ConfigureAppSettings(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddIdentityConfiguration(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddCORSconfiguration(builder.Configuration);
builder.Services.AddAutoMapperConfiguration();
builder.Services.ConfigureIISIntegration();
builder.Services.AddApplicationServices();
builder.Services.AddRepositories();
builder.Services.AddHangfireConfiguration(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
}); ;
builder.Services.AddCustomApiBehavior();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking System API V1");
		c.RoutePrefix = string.Empty;
	});
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseApplicationMiddlewares();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireJobs();
app.MapControllers();
app.Run();
