
namespace CloudGames.Notifications.Application.IntegrationEvents.Purchases
{
    public class PurchaseCreatedIntegrationEvent
    {
        public Guid UserId { get; set; }

        public Guid GameId { get; set; }

        public string Email { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
