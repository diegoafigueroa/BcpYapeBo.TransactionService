using BcpYapeBo.Transaction.Infrastructure.DTOs;
using BcpYapeBo.Transaction.Infrastructure.Repositories;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BcpYapeBo.Transaction.Infrastructure.Messaging
{
    public class TransactionAntiFraudStatusConsumerKafka : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionAntiFraudStatusConsumerKafka> _logger;
        private readonly IConsumer<Null, string> _consumer;
        private readonly string _topic;

        public TransactionAntiFraudStatusConsumerKafka(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<TransactionAntiFraudStatusConsumerKafka> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // RETRIEVE KAFKA CONFIGURATION
            var kafkaSettings = GetKafkaSettings(configuration);

            // PARSE CONSUMER CONFIG USING KAFKASETTINGS
            _consumer = new ConsumerBuilder<Null, string>(kafkaSettings.GetConsumerConfig("transaction-group")).Build();
            _topic = kafkaSettings.TransactionAntiFraudServiceStatusTopic ?? "transaction-anti-fraud-service-status-updated";

            _logger.LogInformation("Kafka consumer initialized with topic {Topic} and servers {Servers}", _topic, kafkaSettings.BootstrapServers);
        }

        private static KafkaSettings GetKafkaSettings(IConfiguration configuration)
        {
            var settings = configuration.GetSection("Kafka").Get<KafkaSettings>();

            // VALIDATE ESSENTIAL FIELDS
            if (string.IsNullOrEmpty(settings.BootstrapServers))
                throw new ArgumentNullException("Kafka servers are not configured.");

            return settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(() => ConsumerLoop(stoppingToken), TaskCreationOptions.LongRunning);
        }

        private void ConsumerLoop(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    var messageValue = result?.Message?.Value;

                    if (string.IsNullOrEmpty(messageValue))
                    {
                        _logger.LogWarning("Received empty message from Kafka topic {Topic}", _topic);
                        continue;
                    }

                    var validation = JsonSerializer.Deserialize<AntiFraudValidation>(messageValue);
                    _logger.LogInformation("Processing transaction {TransactionId} from Kafka topic {Topic}", validation.TransactionExternalId, _topic);

                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

                    var transaction = dbContext.Transactions
                        .FirstOrDefaultAsync(t => t.TransactionExternalId == validation.TransactionExternalId, stoppingToken)
                        .Result;

                    if (transaction != null)
                    {
                        transaction.MarkAsProcessed(validation.Status, validation.RejectionReason);
                        dbContext.SaveChanges();
                        _logger.LogInformation("Transaction {TransactionId} updated successfully", validation.TransactionExternalId);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction {TransactionId} not found in database", validation.TransactionExternalId);
                    }

                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka error while consuming from topic {Topic}: {Error}", _topic, ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing Kafka message from topic {Topic}", _topic);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer for topic {Topic}", _topic);
            _consumer?.Close();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Kafka consumer stopped");
        }
   }
}