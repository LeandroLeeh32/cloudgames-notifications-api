namespace CloudGames.Contracts.IntegrationEvents.Users
{
    public class UserCreatedIntegrationEvent : BaseEvent
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
