namespace CloudGames.Notifications.Application.IntegrationEvents.Users
{
    public class UserCreatedIntegrationEvent : BaseIntegrationEvent
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
