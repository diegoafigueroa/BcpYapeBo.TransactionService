using BcpYapeBo.Transaction.Application.Commands;
using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.Exceptions;
using BcpYapeBo.Transaction.Infrastructure.Messaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using System.Transactions;


namespace BcpYapeBo.Transaction.Infraestructure.Tests
{
    [TestClass]
    public class TransactionAntiFraudServiceKafkaTests
    {
        // MOCKS PARA LOS COMPONENTES NECESARIOS
        private Mock<IProducer<Null, string>> _producerMock;
        private Mock<ILogger<TransactionAntiFraudServiceKafka>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private TransactionAntiFraudServiceKafka _service;
        private const string TestTopic = "test-topic";

        [TestInitialize]
        public void Setup()
        {
            // INICIALIZACIÓN DE LOS MOCKS
            _producerMock = new Mock<IProducer<Null, string>>();
            _loggerMock = new Mock<ILogger<TransactionAntiFraudServiceKafka>>();
            _configMock = new Mock<IConfiguration>();

            // CONFIGURACIÓN SIMULADA DE KAFKA
            _configMock.Setup(c => c["Kafka:BootstrapServers"]).Returns("localhost:9092");
            _configMock.Setup(c => c["Kafka:TransactionAntiFraudServiceValidationTopic"]).Returns(TestTopic);

            // INSTANCIACIÓN DEL SERVICIO CON LOS MOCKS
            _service = new TransactionAntiFraudServiceKafka(_configMock.Object, _loggerMock.Object);
        }

        /// <summary>
        /// PRUEBA QUE EL MÉTODO VALIDATE ENVÍA CORRECTAMENTE UN MENSAJE A KAFKA.
        /// </summary>
        [TestMethod]
        public async Task Validate_ValidTransaction_SendsMessageToKafka()
        {
            // ARRANGE: CREACIÓN DE UNA TRANSACCIÓN VÁLIDA
            var transaction = new BankTransaction(Guid.NewGuid(), Guid.NewGuid(), 1, 100m);
            var jsonMessage = JsonSerializer.Serialize(transaction);

            // SIMULACIÓN DE UN ENVÍO EXITOSO A KAFKA
            var deliveryReport = new DeliveryResult<Null, string>
            {
                Status = PersistenceStatus.Persisted
            };

            _producerMock
                .Setup(p => p.ProduceAsync(TestTopic, It.IsAny<Message<Null, string>>(), default))
                .ReturnsAsync(deliveryReport);

            // ACT: EJECUCIÓN DEL MÉTODO VALIDATE
            await _service.Validate(transaction);

            // ASSERT: VERIFICACIÓN DE QUE EL MENSAJE FUE ENVIADO UNA VEZ
            _producerMock.Verify(
                p => p.ProduceAsync(TestTopic, It.Is<Message<Null, string>>(m => m.Value == jsonMessage), default),
                Times.Once
            );
        }

        /// <summary>
        /// PRUEBA QUE SI KAFKA FALLA AL ENVIAR UN MENSAJE, SE LANZA UNA ANTIFRAUDVALIDATIONEXCEPTION.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AntiFraudValidationException))]
        public async Task Validate_KafkaFails_ThrowsException()
        {
            // ARRANGE: CREACIÓN DE UNA TRANSACCIÓN VÁLIDA
            var transaction = new BankTransaction(Guid.NewGuid(), Guid.NewGuid(), 1, 100m);

            // SIMULACIÓN DE UN ERROR AL ENVIAR EL MENSAJE A KAFKA
            _producerMock
                .Setup(p => p.ProduceAsync(TestTopic, It.IsAny<Message<Null, string>>(), default))
                .ThrowsAsync(new ProduceException<Null, string>(new Error(ErrorCode.Local_QueueFull),, new Exception("Kafka error")));

            // ACT: EJECUCIÓN DEL MÉTODO VALIDATE QUE DEBERÍA LANZAR UNA EXCEPCIÓN
            await _service.Validate(transaction);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA QUE SI KAFKA NO PERSISTE EL MENSAJE, SE LANZA UNA ANTIFRAUDVALIDATIONEXCEPTION.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AntiFraudValidationException))]
        public async Task Validate_MessageNotPersisted_ThrowsException()
        {
            // ARRANGE: CREACIÓN DE UNA TRANSACCIÓN VÁLIDA
            var transaction = new BankTransaction(Guid.NewGuid(), Guid.NewGuid(), 1, 100m);

            // SIMULACIÓN DE QUE EL MENSAJE NO SE PERSISTE
            var deliveryReport = new DeliveryResult<Null, string>
            {
                Status = PersistenceStatus.NotPersisted
            };

            _producerMock
                .Setup(p => p.ProduceAsync(TestTopic, It.IsAny<Message<Null, string>>(), default))
                .ReturnsAsync(deliveryReport);

            // ACT: EJECUCIÓN DEL MÉTODO VALIDATE QUE DEBERÍA LANZAR UNA EXCEPCIÓN
            await _service.Validate(transaction);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA QUE SI LA CONFIGURACIÓN DE KAFKA ES INVÁLIDA, SE LANZA UNA EXCEPCIÓN.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_InvalidKafkaConfiguration_ThrowsException()
        {
            // ARRANGE: CONFIGURACIÓN SIN SERVIDORES KAFKA
            _configMock.Setup(c => c["Kafka:BootstrapServers"]).Returns<string>(null);

            // ACT: INTENTAR CREAR EL SERVICIO DE KAFKA CON CONFIGURACIÓN INVÁLIDA
            new TransactionAntiFraudServiceKafka(_configMock.Object, _loggerMock.Object);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }
    }
}