using CloudGames.Contracts.IntegrationEvents.Users;
using CloudGames.Notifications.Application.UseCases;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CloudGames.Notifications.Infrastructure.Messaging.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly SendWelcomeEmailUseCase _useCase;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(SendWelcomeEmailUseCase useCase, ILogger<UserCreatedConsumer> logger)
        {
            _useCase = useCase;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            try
            {
                var message = context.Message;
                _logger.LogInformation("Received UserCreatedIntegrationEvent for {Email}", message.Email);

                await _useCase.ExecuteAsync(context.Message.Name, context.Message.Email);

                _logger.LogInformation("Welcome email sent to {Email}", message.Email);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error processing message UserCreatedConsumer: {ex.Message}");
                throw;
            }
           

        }
    }
}
