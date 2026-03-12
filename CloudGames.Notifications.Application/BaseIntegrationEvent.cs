using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Application
{
    public abstract class BaseIntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
