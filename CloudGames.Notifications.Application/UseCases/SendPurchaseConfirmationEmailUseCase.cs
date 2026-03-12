using CloudGames.Notifications.Application.IntegrationEvents.Purchases;
using CloudGames.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task ExecuteAsync(string email, decimal price, PaymentStatus status)
        {

            var subject = "Purchase Confirmation";
            var message = $"Your purchase of ${price} was successfully completed.";


            if (status == PaymentStatus.Rejected)
                message = $"Your purchase of ${price} was rejected. Please try again or contact support.";

            await _emailService.SendEmailAsync(email, subject, message);

        }
    }
}
