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

            var subject = "Welcome to CloudGames!";
            var message = $"Hello {name}, welcome to CloudGames!";


            await _emailService.SendEmailAsync(email, subject, message);

        }
    }
}
