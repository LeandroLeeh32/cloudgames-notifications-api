using CloudGames.Notifications.Application.IntegrationEvents.Users;
using CloudGames.Notifications.Application.Interfaces;
using CloudGames.Notifications.Application.UseCases;
using CloudGames.Notifications.Infrastructure.Messaging.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Notifications.Tests.Integration
{
    public class UserCreatedConsumerIntegrationTests
    {
        [Fact]
        public async Task Consumer_Deve_Consumir_Evento()
        {
            var services = new ServiceCollection();

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<UserCreatedConsumer>();
            });

            services.AddScoped<SendWelcomeEmailUseCase>();
            services.AddScoped<IEmailService>(_ => new Mock<IEmailService>().Object);
            services.AddLogging();

            var provider = services.BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            try
            {
                await harness.Bus.Publish(new UserCreatedIntegrationEvent
                {
                    Id = Guid.NewGuid(),
                    Name = "Leandro",
                    Email = "teste@email.com"
                });

                Assert.True(await harness.Consumed.Any<UserCreatedIntegrationEvent>());
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}