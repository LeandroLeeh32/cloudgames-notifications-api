using CloudGames.Notifications.API.Logging;
using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Infrastructure.Services;
using Prometheus;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseCloudGamesLogging("notifications-api");

    Log.Information("Starting CloudGames.Notifications...");

    #region Dependency Injection

    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<SendWelcomeEmailUseCase>();
    builder.Services.AddScoped<SendPurchaseConfirmationEmailUseCase>();

    builder.Services.AddHealthChecks();

    #endregion

    var app = builder.Build();

    app.UseHttpMetrics();

    #region Endpoints

    app.MapGet("/", () => "CloudGames.Notifications API running...");
    app.MapHealthChecks("/health");
    app.MapMetrics();

    #endregion

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application stopped because of exception");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
