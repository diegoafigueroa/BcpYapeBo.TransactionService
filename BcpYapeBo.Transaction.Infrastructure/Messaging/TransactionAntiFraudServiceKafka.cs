using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Domain.Exceptions;

namespace BcpYapeBo.Transaction.Infrastructure.Messaging
{
    // REFERENCE LINKS
    // https://docs.confluent.io/kafka-clients/dotnet/current/overview.html
    // https://developer.confluent.io/get-started/dotnet/#build-producer
    public class TransactionAntiFraudServiceKafka : ITransactionAntiFraudService, IDisposable
    {
        private readonly ILogger<TransactionAntiFraudServiceKafka> _logger;
        private readonly IProducer<Null, string> _kafkaProducer;
        private readonly string _topic;

        public TransactionAntiFraudServiceKafka(IConfiguration configuration, ILogger<TransactionAntiFraudServiceKafka> logger)
        {
            _logger = logger;

            // RETRIEVE KAFKA CONFIGURATION
            var kafkaSettings = GetKafkaSettings(configuration);

            // PARSE PRODUCER CONFIG USING KAFKASETTINGS
            _kafkaProducer = new ProducerBuilder<Null, string>(kafkaSettings.GetProducerConfig()).Build();
            _topic = kafkaSettings.TransactionAntiFraudServiceValidationTopic;

            _logger.LogInformation("Kafka producer initialized with topic {Topic} and servers {Servers}", _topic, kafkaSettings.BootstrapServers);
        }

        public async Task Validate(BankTransaction message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var transactionId = message.TransactionExternalId;

            try
            {
                _logger.LogInformation("Sending transaction {TransactionId} to Kafka topic {Topic}", transactionId, _topic);

                var deliveryResult = await _kafkaProducer.ProduceAsync(_topic, new Message<Null, string> { Value = jsonMessage });
                if (deliveryResult.Status != PersistenceStatus.Persisted)
                {
                    _logger.LogError("Transaction {TransactionId} failed to send to Kafka. Status: {Status}", transactionId, deliveryResult.Status);
                    throw new AntiFraudValidationException($"Failed to send transaction to Kafka. Status: {deliveryResult.Status}");
                }

                _logger.LogInformation("Transaction {TransactionId} successfully sent to Kafka topic {Topic}", transactionId, _topic);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Kafka error while sending transaction {TransactionId}: {Error}", transactionId, ex.Error.Reason);
                throw new AntiFraudValidationException($"Kafka error: {ex.Error.Reason}", ex);
            }
        }

        private static KafkaSettings GetKafkaSettings(IConfiguration configuration)
        {
            var settings = configuration.GetSection("Kafka").Get<KafkaSettings>();

            // VALIDATE ESSENTIAL FIELDS
            if (string.IsNullOrEmpty(settings.BootstrapServers))
                throw new ArgumentNullException("Kafka servers are not configured.");

            if (string.IsNullOrEmpty(settings.TransactionAntiFraudServiceValidationTopic))
                throw new ArgumentNullException("Kafka topic is not configured.");

            return settings;
        }

        public void Dispose()
        {
            _logger.LogInformation("Flushing and disposing Kafka producer.");
            _kafkaProducer?.Flush(TimeSpan.FromSeconds(10));
            _kafkaProducer?.Dispose();
            _logger.LogInformation("Kafka producer disposed.");
        }
    }
}