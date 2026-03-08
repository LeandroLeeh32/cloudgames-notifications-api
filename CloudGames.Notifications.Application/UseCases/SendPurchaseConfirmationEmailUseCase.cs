using CloudGames.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Application.UseCases
{
    public class SendPurchaseConfirmationEmailUseCase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<SendPurchaseConfirmationEmailUseCase> _logger;

        public SendPurchaseConfirmationEmailUseCase(
            IEmailService emailService,
            ILogger<SendPurchaseConfirmationEmailUseCase> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ExecuteAsync(string email, decimal amount)
        {

            var subject = "Purchase Confirmation";
            var message = $"Your purchase of ${amount} was successfully completed.";


            _logger.LogInformation(
                "Sending purchase confirmation email to {Email} for amount {Amount}", email, amount);

            await _emailService.SendEmailAsync(email, subject, message);

            _logger.LogInformation("Purchase confirmation email successfully sent to {Email}", email);
        }
    }
}
