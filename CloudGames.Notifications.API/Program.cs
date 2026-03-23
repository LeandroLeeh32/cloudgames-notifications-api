using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Infrastructure.Configuration;
using CloudGames.Notifications.Infrastructure.Messaging.Consumers;
using CloudGames.Notifications.Infrastructure.Services;
using FIAP.Messages;
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

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    #endregion

    #region Configuration

    builder.Services.Configure<MassTransitSettings>(builder.Configuration.GetSection("RabbitMQ"));

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
            var settings = context.GetRequiredService<IOptions<MassTransitSettings>>().Value;

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq-service";

            var rabbitVirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUAL_HOST")
                                     ?? settings.VirtualHost
                                     ?? "/";

            var rabbitUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")
                                 ?? settings.Username
                                 ?? throw new InvalidOperationException("RABBITMQ_USERNAME não configurado.");

            var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")
                                 ?? settings.Password
                                 ?? throw new InvalidOperationException("RABBITMQ_PASSWORD não configurado.");

            var userCreatedQueue = Environment.GetEnvironmentVariable("USER_CREATED_QUEUE")
                                   ?? settings.Queues.UserCreated
                                   ?? throw new InvalidOperationException("USER_CREATED_QUEUE não configurada.");

            var paymentProcessedQueue = Environment.GetEnvironmentVariable("PURCHASE_CREATED_QUEUE")
                                       ?? settings.Queues.PurchaseCreated
                                       ?? throw new InvalidOperationException("PURCHASE_CREATED_QUEUE não configurada.");

            logger.Info("PROGRAM NOVA DO NOTIFICATIONS");
            logger.Info($"RabbitMQ Host: {rabbitHost}");
            logger.Info($"UserCreated Queue: {userCreatedQueue}");
            logger.Info($"PurchaseCreated Queue: {paymentProcessedQueue}");

            cfg.Host(rabbitHost, rabbitVirtualHost, h =>
            {
                h.Username(rabbitUsername);
                h.Password(rabbitPassword);
            });

            cfg.ReceiveEndpoint(userCreatedQueue, e =>
            {
                e.ConfigureConsumer<UserCreatedConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Interval(settings.RetryCount, TimeSpan.FromSeconds(settings.RetryIntervalSeconds));
                });
            });

            cfg.Message<PaymentProcessedEvent>(x => x.SetEntityName("PaymentProcessedEvent"));

            cfg.ReceiveEndpoint(paymentProcessedQueue, e =>
            {
                e.ConfigureConsumer<PurchaseCreatedConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Interval(settings.RetryCount, TimeSpan.FromSeconds(settings.RetryIntervalSeconds));
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