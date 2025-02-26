using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Ports.Driving;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.Domain.Entities;
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
    // REFERENCE:
    // https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service
    public class TransactionAntiFraudStatusConsumerKafka : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TransactionAntiFraudStatusConsumerKafka> _logger;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly string _topic;

        public TransactionAntiFraudStatusConsumerKafka(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<TransactionAntiFraudStatusConsumerKafka> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            // RETRIEVE KAFKA CONFIGURATION
            var kafkaSettings = GetKafkaSettings(configuration);

            // PARSE CONSUMER CONFIG USING KAFKASETTINGS
            _kafkaConsumer = new ConsumerBuilder<Null, string>(kafkaSettings.GetConsumerConfig("transaction-group")).Build();
            _topic = kafkaSettings.TransactionAntiFraudServiceStatusTopic ?? "transaction-anti-fraud-service-status-updated";

            _logger.LogInformation("Kafka consumer initialized with topic {Topic} and servers {Servers}", _topic, kafkaSettings.BootstrapServers);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(() => ConsumerLoop(stoppingToken), TaskCreationOptions.LongRunning);
        }

        private void ConsumerLoop(CancellationToken stoppingToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _kafkaConsumer.Consume(TimeSpan.FromMilliseconds(100));
                    var messageValue = result?.Message?.Value;

                    // VALIDA QUE EL MENSAJE NO SEA NULO O VACÍO.
                    if (string.IsNullOrEmpty(messageValue))
                    {
                        _logger.LogWarning("Received empty message from Kafka topic {Topic}", _topic);
                        continue;
                    }

                    // DESERIALIZA EL MENSAJE RECIBIDO DESDE KAFKA.
                    var validationResult = JsonSerializer.Deserialize<AntiFraudValidationResult>(messageValue);
                    _logger.LogInformation("Processing transaction {TransactionId} from Kafka topic {Topic}", validationResult.TransactionExternalId, _topic);

                    // ACTUALIZA EL ESTADO DE LA TRANSACCIÓN CON EL RESULTADO DE LA VALIDACIÓN ANTIFRAUDE.
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // OBTIENE EL SERVICIO DE TRANSACCIÓN DESDE EL SCOPE Y ACTULIZA EL ESTADO
                        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                        transactionService.UpdateTransactionStatusWithAntiFraudCheckAsync(validationResult);
                    }

                    _kafkaConsumer.Commit(result);
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

        private static KafkaSettings GetKafkaSettings(IConfiguration configuration)
        {
            var settings = configuration.GetSection("Kafka").Get<KafkaSettings>();

            // VALIDATE ESSENTIAL FIELDS
            if (string.IsNullOrEmpty(settings.BootstrapServers))
                throw new ArgumentNullException("Kafka servers are not configured.");

            return settings;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer for topic {Topic}", _topic);
            _kafkaConsumer?.Close();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Kafka consumer stopped");
        }

    }
}