using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Functions.Logging;
using CloudGames.Notifications.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

// Configura Serilog com Redis Stream — mesmo padrão dos outros serviços (users, catalog, payments)
// Os logs aparecem no Grafana em cloudgames:logs automaticamente
var redisConnection = Environment.GetEnvironmentVariable("Redis__ConnectionString") ?? "redis-service:6379";
var streamKey      = Environment.GetEnvironmentVariable("Redis__LogStream")         ?? "cloudgames:logs";

var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System",    LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console();

try
{
    var options = ConfigurationOptions.Parse(redisConnection);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 1500;
    var redis = ConnectionMultiplexer.Connect(options);
    logConfig.WriteTo.Sink(new RedisStreamSink(redis, streamKey, "notifications-functions"));
}
catch
{
    // Redis indisponível: continua apenas com Console
}

Log.Logger = logConfig.CreateLogger();

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Adiciona Serilog sem remover os providers internos do Azure Functions
builder.Logging.AddSerilog(Log.Logger, dispose: true);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SendWelcomeEmailUseCase>();
builder.Services.AddScoped<SendPurchaseConfirmationEmailUseCase>();

builder.Build().Run();
