using System.Text.Json;
using System.Text.Json.Nodes;
using CloudGames.Notifications.Application.IntegrationEvents.Users;
using CloudGames.Notifications.Application.UseCases;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CloudGames.Notifications.Functions.Functions;

public class UserCreatedFunction
{
    private readonly SendWelcomeEmailUseCase _useCase;
    private readonly ILogger<UserCreatedFunction> _logger;

    public UserCreatedFunction( SendWelcomeEmailUseCase useCase,ILogger<UserCreatedFunction> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [Function("UserCreatedFunction")]
    public async Task Run([RabbitMQTrigger("UserCreated",ConnectionStringSetting = "RabbitMQConnection")]string message)
    {
        try
        {
            _logger.LogInformation("Starting processing UserCreated event");

            var json = JsonNode.Parse(message);
            var payload = json?["message"];

            var userEvent = payload?.Deserialize<UserCreatedIntegrationEvent>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (userEvent is null)
            {
                _logger.LogError("Erro ao desserializar evento");
                return;
            }

            _logger.LogInformation("Processing user creation for Email: {Email}",userEvent.Email);

            await _useCase.ExecuteAsync(userEvent.Name,userEvent.Email);

            _logger.LogInformation("UserCreated event processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error processing UserCreated event");
            throw;
        }
    }
}