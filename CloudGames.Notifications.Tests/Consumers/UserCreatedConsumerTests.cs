using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Contracts.IntegrationEvents.Users;
using CloudGames.Notifications.Infrastructure.Messaging.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace CloudGames.Notifications.Tests.Consumers
{
    public class UserCreatedConsumerTests
    {
        [Fact]
        public async Task Consumer_Deve_Executar_UseCase()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var useCaseLoggerMock = new Mock<ILogger<SendWelcomeEmailUseCase>>();
            var consumerLoggerMock = new Mock<ILogger<UserCreatedConsumer>>();

            var useCase = new SendWelcomeEmailUseCase( emailServiceMock.Object, useCaseLoggerMock.Object);

            var consumer = new UserCreatedConsumer(useCase, consumerLoggerMock.Object);

            var message = new UserCreatedIntegrationEvent
            {
                Id = Guid.NewGuid(),
                Name = "Leandro",
                Email = "leandro@email.com"
            };

            var context = Mock.Of<ConsumeContext<UserCreatedIntegrationEvent>>(x => x.Message == message);

            await consumer.Consume(context);

            emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    message.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}