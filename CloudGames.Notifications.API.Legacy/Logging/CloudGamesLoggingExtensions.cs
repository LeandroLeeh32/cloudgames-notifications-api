using Serilog;
using Serilog.Events;
using StackExchange.Redis;

namespace CloudGames.Notifications.API.Logging
{
    public static class CloudGamesLoggingExtensions
    {
        public static IHostBuilder UseCloudGamesLogging(this IHostBuilder builder, string serviceName)
        {
            return builder.UseSerilog((context, services, configuration) =>
            {
                var redisConnection = Environment.GetEnvironmentVariable("Redis__ConnectionString")
                                      ?? context.Configuration["Redis:ConnectionString"]
                                      ?? "redis:6379";

                var streamKey = Environment.GetEnvironmentVariable("Redis__LogStream")
                                ?? context.Configuration["Redis:LogStream"]
                                ?? "cloudgames:logs";

                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("service", serviceName)
                    .WriteTo.Console();

                try
                {
                    var options = ConfigurationOptions.Parse(redisConnection);
                    if (!redisConnection.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
                        options.AbortOnConnectFail = false;
                    if (!redisConnection.Contains("connectTimeout", StringComparison.OrdinalIgnoreCase))
                        options.ConnectTimeout = 1500;

                    var redis = ConnectionMultiplexer.Connect(options);
                    configuration.WriteTo.Sink(new RedisStreamSink(redis, streamKey, serviceName));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Logging] Falha ao conectar ao Redis ({redisConnection}): {ex.Message}. Apenas Console será usado.");
                }
            });
        }
    }
}
