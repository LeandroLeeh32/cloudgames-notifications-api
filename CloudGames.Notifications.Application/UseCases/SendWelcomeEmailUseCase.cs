using CloudGames.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CloudGames.Notifications.Application.UseCases
{
    public class SendWelcomeEmailUseCase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<SendWelcomeEmailUseCase> _logger;

        public SendWelcomeEmailUseCase(IEmailService emailService,ILogger<SendWelcomeEmailUseCase> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }


        public async Task ExecuteAsync(string name, string email)
        {
            _logger.LogInformation("Sending welcome email to {Email}", email);

            var subject = "Welcome to CloudGames!";
            var message = $"Hello {name}, welcome to CloudGames!";

            //throw new Exception("Erro proposital");

            await _emailService.SendEmailAsync(email, subject, message);

            _logger.LogInformation("Email successfully sent to {Email}", email);
        }
    }
}
