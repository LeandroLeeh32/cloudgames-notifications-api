using CloudGames.Notifications.Application.IntegrationEvents.Users;
using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace CloudGames.Notifications.Tests.Consumers
{
    public class UserCreatedFunctionTests
    {
        [Fact]
        public async Task UseCase_Deve_Enviar_Email_Ao_Criar_Usuario()
        {
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<SendWelcomeEmailUseCase>>();

            var useCase = new SendWelcomeEmailUseCase(emailServiceMock.Object, loggerMock.Object);

            await useCase.ExecuteAsync("Leandro", "leandro@email.com");

            emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    "leandro@email.com",
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
