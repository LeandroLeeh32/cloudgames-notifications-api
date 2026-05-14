using CloudGames.Notifications.Application.IntegrationEvents.Purchases;
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
    public async Task Run( [RabbitMQTrigger( "PaymentProcessed",ConnectionStringSetting = "RabbitMQConnection")] string message)
    {

        var json = JsonNode.Parse(message);
        var payload = json?["message"];

        var paymentEvent = payload?.Deserialize<PaymentProcessedEvent>(new JsonSerializerOptions
       {
           PropertyNameCaseInsensitive = true,
           NumberHandling = JsonNumberHandling.AllowReadingFromString
       });

        if (paymentEvent is null)
        {
            _logger.LogError(
                "Erro ao desserializar PaymentProcessedEvent");

            return;
        }

        await _useCase.ExecuteAsync(paymentEvent.Email,paymentEvent.Price,paymentEvent.Status);

    }
}