using Confluent.Kafka;

namespace BcpYapeBo.Transaction.Infrastructure.Messaging
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string TransactionAntiFraudServiceValidationTopic { get; set; }
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
        public string SecurityProtocol { get; set; }
        public string SaslMechanism { get; set; }

        public ProducerConfig GetProducerConfig()
        {
            // CONFIGURACIÓN BÁSICA
            var config = new ProducerConfig
            {
                BootstrapServers = BootstrapServers,
                Acks = Acks.All // CONFIRMACIÓN DE TODAS LAS RÉPLICAS
            };

            // SI HAY CREDENCIALES, LAS AÑADIMOS
            if (!string.IsNullOrEmpty(SaslUsername) && !string.IsNullOrEmpty(SaslPassword))
            {
                config.SaslUsername = SaslUsername;
                config.SaslPassword = SaslPassword;
                config.SecurityProtocol = ParseOrDefault(SecurityProtocol, Confluent.Kafka.SecurityProtocol.Plaintext);
                config.SaslMechanism = ParseOrDefault(SaslMechanism, Confluent.Kafka.SaslMechanism.Plain);
            }

            return config;
        }

        private static T ParseOrDefault<T>(string value, T defaultValue) where T : struct
        {
            return Enum.TryParse(value, out T result) ? result : defaultValue;
        }
    }
}
