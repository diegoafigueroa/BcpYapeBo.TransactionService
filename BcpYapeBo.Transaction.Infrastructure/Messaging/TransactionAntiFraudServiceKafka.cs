using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Application.Ports.Driven;

namespace BcpYapeBo.Transaction.Infrastructure.Messaging
{
    public class TransactionAntiFraudServiceKafka : ITransactionAntiFraudService, IDisposable
    {
        private readonly IProducer<Null, string> _kafkaProducer;
        private readonly string _topic;
        private readonly ILogger<TransactionAntiFraudServiceKafka> _logger;

        public TransactionAntiFraudServiceKafka(IConfiguration configuration)
        {
            _topic = configuration["Kafka:TransactionAntiFraudServiceValidationTopic"];

            // CONFIABILIDAD: GARANTIZAR QUE TODAS LAS REPLICAS LO RECIBAN ..
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All 
            };

            _kafkaProducer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task Validate(BankTransaction message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var deliveryResult = await _kafkaProducer.ProduceAsync(_topic, new Message<Null, string> { Value = jsonMessage });

                if (deliveryResult.Status != PersistenceStatus.Persisted)
                    throw new Exception("Failed to send transaction to Kafka");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al producir mensaje en Kafka: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _kafkaProducer?.Flush(TimeSpan.FromSeconds(10));
            _kafkaProducer?.Dispose();
        }
    }
}
