namespace CloudGames.Contracts.IntegrationEvents.Purchases
{
    public class PurchaseCreatedIntegrationEvent
    {
        public Guid PurchaseId { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
    }
}
