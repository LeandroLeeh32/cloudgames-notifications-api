using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Infrastructure.Configuration;
using CloudGames.Notifications.Infrastructure.Messaging.Consumers;
using CloudGames.Notifications.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;

#region Logging

var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

#endregion

try
{
    logger.Info("Starting CloudGames.Notifications...");

    #region Builder

    var builder = WebApplication.CreateBuilder(args);

    // Remove todos os loggers padrão
    builder.Logging.ClearProviders();

    // Usa somente NLog
    builder.Host.UseNLog();

    #endregion


    #region Configuration

    builder.Services.Configure<MassTransitSettings>(
        builder.Configuration.GetSection("MassTransit"));

    #endregion


    #region Dependency Injection

    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<SendWelcomeEmailUseCase>();
    builder.Services.AddScoped<SendPurchaseConfirmationEmailUseCase>();

    builder.Services.AddHealthChecks();

    #endregion


    #region MassTransit

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<UserCreatedConsumer>();
        x.AddConsumer<PurchaseCreatedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var settings = context
                .GetRequiredService<IOptions<MassTransitSettings>>().Value;

            cfg.Host(settings.Host, settings.VirtualHost, h =>
            {
                h.Username(settings.Username);
                h.Password(settings.Password);
            });

            cfg.ReceiveEndpoint(settings.Queues.UserCreated, e =>
            {
                e.ConfigureConsumer<UserCreatedConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Interval(settings.RetryCount,
                        TimeSpan.FromSeconds(settings.RetryIntervalSeconds));
                });
            });

            cfg.ReceiveEndpoint(settings.Queues.PurchaseCreated, e =>
            {
                e.ConfigureConsumer<PurchaseCreatedConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Interval(settings.RetryCount,
                        TimeSpan.FromSeconds(settings.RetryIntervalSeconds));
                });
            });
        });
    });

    #endregion


    #region Build App

    var app = builder.Build();

    #endregion


    #region Endpoints

    app.MapGet("/", () => "CloudGames.Notifications API running...");
    app.MapHealthChecks("/health");

    #endregion

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}