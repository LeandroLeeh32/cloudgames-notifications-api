using CloudGames.Notifications.Application;
using CloudGames.Notifications.Application.IntegrationEvents.Purchases;

namespace FIAP.Messages;

public class PaymentProcessedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PaymentStatus Status { get; set; }
}

