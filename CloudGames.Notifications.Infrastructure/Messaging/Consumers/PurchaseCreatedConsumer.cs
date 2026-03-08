
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Contracts.IntegrationEvents.Purchases;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CloudGames.Notifications.Infrastructure.Messaging.Consumers
{
    public class PurchaseCreatedConsumer : IConsumer<PurchaseCreatedIntegrationEvent>
    {
        private readonly SendPurchaseConfirmationEmailUseCase _useCase;
        private readonly ILogger<PurchaseCreatedConsumer> _logger;

        public PurchaseCreatedConsumer(
            SendPurchaseConfirmationEmailUseCase useCase,
            ILogger<PurchaseCreatedConsumer> logger)
        {
            _useCase = useCase;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PurchaseCreatedIntegrationEvent> context)
        {
            try
            {
                _logger.LogInformation(
                "Received PurchaseCreatedIntegrationEvent for {Email}",
                context.Message.Email);

                await _useCase.ExecuteAsync(context.Message.Email, context.Message.Amount);
            }

            catch (Exception ex)
            {

                Console.WriteLine($"Error processing message PurchaseCreatedConsumer: {ex.Message}");
                throw;
            }
        }
    }
}
