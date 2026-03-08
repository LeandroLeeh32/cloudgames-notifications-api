using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Infrastructure.Configuration
{
    public class MassTransitSettings
    {
        public string Host { get; set; } = default!;

        public string VirtualHost { get; set; } = "/";

        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public int RetryCount { get; set; }

        public int RetryIntervalSeconds { get; set; }

        public QueueSettings Queues { get; set; } = new();
    }
    public class QueueSettings
    {
        public string UserCreated { get; set; } = default!;

        public string PurchaseCreated { get; set; } = default!;
    }
}
