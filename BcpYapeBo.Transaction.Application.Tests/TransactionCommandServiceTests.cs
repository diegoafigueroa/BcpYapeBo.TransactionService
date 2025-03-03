using BcpYapeBo.Transaction.Application.Commands;
using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.Exceptions;
using Moq;
using System.Transactions;

namespace BcpYapeBo.Transaction.Application.Tests
{
    [TestClass]
    public class TransactionCommandServiceHandleTests
    {
        private Mock<ITransactionRepository> _transactionRepositoryMock;
        private Mock<ITransactionAntiFraudService> _transactionAntiFraudServiceMock;
        private TransactionCommandService _service;

        [TestInitialize]
        public void Setup()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _transactionAntiFraudServiceMock = new Mock<ITransactionAntiFraudService>();
            _service = new TransactionCommandService(_transactionRepositoryMock.Object, _transactionAntiFraudServiceMock.Object);
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA EL MÉTODO HANDLE, VERIFICANDO QUE UNA TRANSACCIÓN VÁLIDA SE CREA CORRECTAMENTE.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidTransaction_CreatesTransaction()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var value = 100m;

            // SE CREA UN COMANDO CON LOS DATOS DE LA TRANSACCIÓN
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, validTransactionType, value);

            // SE CONFIGURA EL REPOSITORIO MOCK PARA SIMULAR LA PERSISTENCIA DE LA TRANSACCIÓN SIN ERRORES
            _transactionRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<BankTransaction>()))
                .Returns(Task.CompletedTask);

            // SE CONFIGURA EL SERVICIO ANTI-FRAUDE MOCK PARA SIMULAR LA VALIDACIÓN SIN ERRORES
            _transactionAntiFraudServiceMock
                .Setup(s => s.Validate(It.IsAny<BankTransaction>()))
                .Returns(Task.CompletedTask);

            // ACT
            // Se ejecuta el método Handle con el comando creado
            var transaction = await _service.Handle(command);

            // ASSERT
            // Se verifica que la transacción generada no sea nula
            Assert.IsNotNull(transaction);

            // Se valida que los datos de la transacción generada sean los esperados
            Assert.AreEqual(sourceAccountId, transaction.SourceAccountId.Value);
            Assert.AreEqual(targetAccountId, transaction.TargetAccountId.Value);
            Assert.AreEqual(validTransactionType, (int)transaction.Type);
            Assert.AreEqual(value, transaction.Value.Amount);

            // Se verifica que el repositorio guardó la transacción exactamente una vez
            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<BankTransaction>()), Times.Once);

            // Se verifica que el servicio anti-fraude validó la transacción exactamente una vez
            _transactionAntiFraudServiceMock.Verify(s => s.Validate(It.IsAny<BankTransaction>()), Times.Once);
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO SE ENVÍA UN TIPO DE TRANSACCIÓN INVÁLIDO.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PropertyValidationException))]
        public async Task Handle_InvalidTransactionType_ThrowsArgumentException()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var invalidTransactionType = 99;
            var value = 100m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, invalidTransactionType, value);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO EL ID DE LA CUENTA DE ORIGEN ES GUID.EMPTY.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PropertyValidationException))]
        public async Task Handle_EmptySourceAccountId_ThrowsPropertyValidationException()
        {
            // ARRANGE
            var emptySource = Guid.Empty;
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var value = 100m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(targetAccountId, emptySource, validTransactionType, value);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO EL ID DE LA CUENTA DE DESTINO ES GUID.EMPTY.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PropertyValidationException))]
        public async Task Handle_EmptyTargetAccountId_ThrowsPropertyValidationException()
        {
            // ARRANGE
            Guid sourceAccountId = Guid.NewGuid();
            Guid emptyTarget = Guid.Empty;
            int validTransactionType = 1;
            decimal value = 100m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(sourceAccountId, emptyTarget, validTransactionType, value);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO EL MONTO DE LA TRANSACCIÓN NO ES POSITIVO.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PropertyValidationException))]
        public async Task Handle_NonPositiveValue_ThrowsPropertyValidationException()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var invalidValue = 0m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, validTransactionType, invalidValue);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO EL MONTO DE LA TRANSACCIÓN ES NEGATIVO.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PropertyValidationException))]
        public async Task Handle_NegativeTransactionValue_ThrowsPropertyValidationException()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var negativeValue = -50m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, validTransactionType, negativeValue);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN CUANDO LA CUENTA DE ORIGEN Y LA CUENTA DE DESTINO SON LA MISMA.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BusinessRuleException))]
        public async Task Handle_SameSourceAndTargetAccount_ThrowsBusinessRuleException()
        {
            // ARRANGE
            var accountId = Guid.NewGuid();
            var validTransactionType = 1;
            var value = 100m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(accountId, accountId, validTransactionType, value);

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

        [TestMethod]
        public async Task Handle_ValidTransaction_PersistsTransaction()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var value = 100m;

            // Se crea un comando con los datos de la transacción inválida
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, validTransactionType, value);

            _transactionRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<BankTransaction>()))
                .Returns(Task.CompletedTask);

            // ACT
            await _service.Handle(command);

            // ASSERT
            _transactionRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<BankTransaction>()), Times.Once);
        }

        /// <summary>
        /// PRUEBA UNITARIA PARA VERIFICAR QUE SE LANZA UNA EXCEPCIÓN SI LA VALIDACIÓN ANTIFRAUDE FALLA.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AntiFraudValidationException))]
        public async Task Handle_FraudulentTransaction_ThrowsAntiFraudException()
        {
            // ARRANGE
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            var validTransactionType = 1;
            var value = 100m;

            // SE CREA UN COMANDO CON LOS DATOS DE LA TRANSACCIÓN
            var command = new CreateTransactionCommand(sourceAccountId, targetAccountId, validTransactionType, value);

            // SE CONFIGURA EL SERVICIO ANTI-FRAUDE MOCK PARA SIMULAR UN ERROR EN EL ENVIO DE LA TRANSACCIÓN A KAFKA
            _transactionAntiFraudServiceMock
                .Setup(s => s.Validate(It.IsAny<BankTransaction>()))
                .Throws(new AntiFraudValidationException("Transaction {TransactionId} failed to send to Kafka"));

            // ACT
            // Se ejecuta el método Handle, esperando que lance una excepción
            await _service.Handle(command);

            // ASSERT -> LA EXCEPCIÓN SE VERIFICA CON [ExpectedException]
        }

    }
}