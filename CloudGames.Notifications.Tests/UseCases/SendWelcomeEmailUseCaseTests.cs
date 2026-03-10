using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Tests.UseCases
{
    public class SendWelcomeEmailUseCaseTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<SendWelcomeEmailUseCase>> _loggerMock;
        private readonly SendWelcomeEmailUseCase _useCase;

        public SendWelcomeEmailUseCaseTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<SendWelcomeEmailUseCase>>();

            _useCase = new SendWelcomeEmailUseCase(
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Deve_Enviar_Email()
        {
            var name = "Leandro";
            var email = "leandro@email.com";

            var subject = "Welcome to CloudGames!";
            var message = $"Hello {name}, welcome to CloudGames!";

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(email, subject, message))
                .Returns(Task.CompletedTask);

            await _useCase.ExecuteAsync(name, email);

            _emailServiceMock.Verify(
                x => x.SendEmailAsync(email, subject, message),
                Times.Once);
        }
    }
}
