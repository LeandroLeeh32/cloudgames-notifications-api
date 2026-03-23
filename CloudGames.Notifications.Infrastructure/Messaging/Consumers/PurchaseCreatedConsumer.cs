
using CloudGames.Notifications.Application.IntegrationEvents.Purchases;
using CloudGames.Notifications.Application.UseCases;
using FIAP.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CloudGames.Notifications.Infrastructure.Messaging.Consumers
{
    public class PurchaseCreatedConsumer : IConsumer<PaymentProcessedEvent>
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

        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var message = context.Message;
            try
            {

                Console.WriteLine($"Processing purchase event for user {message.UserId.ToString()}");

                await _useCase.ExecuteAsync(
                    message.Email,
                    message.Price,
                    message.Status
                  );

            }

            catch (Exception ex)
            {
                Console.WriteLine(@"Purchase rejected for user {UserId}", message.UserId);
                Console.WriteLine($"Error processing message PurchaseCreatedConsumer: {ex.Message}");
                throw;
            }
        }
    }
}
