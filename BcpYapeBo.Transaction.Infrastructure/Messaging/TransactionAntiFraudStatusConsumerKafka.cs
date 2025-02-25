using BcpYapeBo.Transaction.Infrastructure.DTOs;
using BcpYapeBo.Transaction.Infrastructure.Repositories;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace BcpYapeBo.Transaction.Infrastructure.Messaging
{
    public class TransactionAntiFraudStatusConsumerKafka : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsumer<Null, string> _consumer;

        public TransactionAntiFraudStatusConsumerKafka(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "transaction-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(() => ConsumerLoop(stoppingToken), TaskCreationOptions.LongRunning);
        }

        private void ConsumerLoop(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("transaction-anti-fraud-service-status-updated");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    var validation = JsonSerializer.Deserialize<AntiFraudValidation>(result.Message.Value);

                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

                    var transaction = dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionExternalId == validation.TransactionExternalId, stoppingToken).Result;

                    if (transaction != null)
                    {
                        transaction.MarkAsProcessed(validation.Status, validation.RejectionReason);
                        dbContext.SaveChanges();
                    }

                    _consumer.Commit(result);
                }
                catch (Exception ex)
                {
                }
            }
        }

    }
}