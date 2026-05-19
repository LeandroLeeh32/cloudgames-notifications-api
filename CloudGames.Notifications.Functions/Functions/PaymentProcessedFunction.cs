using CloudGames.Notifications.Application.UseCases;
using FIAP.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CloudGames.Notifications.Functions.Functions;

public class PaymentProcessedFunction
{
    private readonly SendPurchaseConfirmationEmailUseCase _useCase;
    private readonly ILogger<PaymentProcessedFunction> _logger;

    public PaymentProcessedFunction(
        SendPurchaseConfirmationEmailUseCase useCase,
        ILogger<PaymentProcessedFunction> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [Function("PaymentProcessedFunction")]
    public async Task Run([RabbitMQTrigger("PaymentProcessed", ConnectionStringSetting = "RabbitMQConnection")]string message)
    {
        try
        {
            _logger.LogInformation("Starting processing PaymentProcessed event");

            var json = JsonNode.Parse(message);
            var payload = json?["message"];

            var paymentEvent = payload?.Deserialize<PaymentProcessedEvent>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });

            if (paymentEvent is null)
            {
                _logger.LogError("Erro ao desserializar PaymentProcessedEvent");
                return;
            }

            _logger.LogInformation("Processing payment for Email: {Email} - Status: {Status}",paymentEvent.Email,paymentEvent.Status);

            await _useCase.ExecuteAsync(paymentEvent.Email,paymentEvent.Price,paymentEvent.Status);

            _logger.LogInformation("PaymentProcessed event processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error processing PaymentProcessed event");
            throw;
        }
    }
}