using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace CloudGames.Notifications.Tests.UseCases
{
    public class SendPurchaseConfirmationEmailUseCaseTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<SendPurchaseConfirmationEmailUseCase>> _loggerMock;
        private readonly SendPurchaseConfirmationEmailUseCase _useCase;

        public SendPurchaseConfirmationEmailUseCaseTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<SendPurchaseConfirmationEmailUseCase>>();

            _useCase = new SendPurchaseConfirmationEmailUseCase(
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Deve_Enviar_Email_De_Confirmacao()
        {
            var email = "cliente@email.com";
            var amount = 100;

            var subject = "Purchase Confirmation";
            var message = $"Your purchase of ${amount} was successfully completed.";

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(email, subject, message))
                .Returns(Task.CompletedTask);

            await _useCase.ExecuteAsync(email, amount);

            _emailServiceMock.Verify(
                x => x.SendEmailAsync(email, subject, message),
                Times.Once);
        }
    }
}