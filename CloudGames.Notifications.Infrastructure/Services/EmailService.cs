using CloudGames.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("Sending email to {Email} - {Subject}", email, subject);

            Console.WriteLine($"To: {email}");
            Console.WriteLine($"subject: {subject}");
            Console.WriteLine($"message: {message}");

            await Task.CompletedTask;
        }
    }
}
