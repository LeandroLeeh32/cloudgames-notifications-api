using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace CloudGames.Notifications.API.Logging
{
    public class RedisStreamSink : ILogEventSink
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _streamKey;
        private readonly string _serviceName;
        private const int MaxStreamLength = 100_000;

        public RedisStreamSink(IConnectionMultiplexer redis, string streamKey, string serviceName)
        {
            _redis = redis;
            _streamKey = streamKey;
            _serviceName = serviceName;
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                var db = _redis.GetDatabase();

                var properties = logEvent.Properties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString().Trim('"'));

                var entries = new[]
                {
                    new NameValueEntry("timestamp", logEvent.Timestamp.ToString("O")),
                    new NameValueEntry("level", logEvent.Level.ToString()),
                    new NameValueEntry("service", _serviceName),
                    new NameValueEntry("message", logEvent.RenderMessage()),
                    new NameValueEntry("template", logEvent.MessageTemplate.Text),
                    new NameValueEntry("properties", JsonSerializer.Serialize(properties)),
                    new NameValueEntry("exception", logEvent.Exception?.ToString() ?? string.Empty)
                };

                db.StreamAdd(_streamKey, entries, maxLength: MaxStreamLength, useApproximateMaxLength: true);
            }
            catch
            {
                // Não derrubar a aplicação por falha de logging
            }
        }
    }
}
