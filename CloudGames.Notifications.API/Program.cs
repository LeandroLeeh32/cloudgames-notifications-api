using CloudGames.Notifications.API.Logging;
using CloudGames.Notifications.Application.IntegrationEvents.Users;
using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Infrastructure.Configuration;
using CloudGames.Notifications.Infrastructure.Messaging.Consumers;
using CloudGames.Notifications.Infrastructure.Services;
using FIAP.Messages;
using MassTransit;
using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseCloudGamesLogging("notifications-api");

    Log.Information("Starting CloudGames.Notifications...");

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

            Log.Information("RabbitMQ Host: {RabbitHost}", rabbitHost);
            Log.Information("UserCreated Queue: {UserCreatedQueue}", userCreatedQueue);
            Log.Information("PurchaseCreated Queue: {PurchaseCreatedQueue}", paymentProcessedQueue);

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

            cfg.ReceiveEndpoint(paymentProcessedQueue, e =>
            {
                e.ConfigureConsumer<PurchaseCreatedConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Interval(settings.RetryCount, TimeSpan.FromSeconds(settings.RetryIntervalSeconds));
                });
            });


            cfg.Message<PaymentProcessedEvent>(x => x.SetEntityName("PaymentProcessedEvent"));
            cfg.Message<UserCreatedIntegrationEvent>(x => x.SetEntityName("UserCreatedIntegrationEvent"));
        });
    });

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
