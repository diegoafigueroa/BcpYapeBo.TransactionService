using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Ports.Driving;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.ValueObjects;
using System.Transactions;

namespace BcpYapeBo.Transaction.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionAntiFraudService _transactionAntiFraudService;

        public TransactionService(ITransactionRepository transactionRepository, ITransactionAntiFraudService transactionAntiFraudService)
        {
            _transactionRepository = transactionRepository;
            _transactionAntiFraudService = transactionAntiFraudService;
        }

        public async Task<Guid> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
        {
            var bankTransaction = new BankTransaction(
                sourceAccountId: sourceAccountId,
                targetAccountId: targetAccountId,
                transactionTypeId: transferTypeId,
                value: value
            );

            await _transactionRepository.SaveAsync(bankTransaction);

            // ENVÍO DE LA TRANSACCIÓN AL SERVICIO DE ANTI-FRAUDE A TRAVÉS DE UN SERVICIO DE MENSAJERIA EN TIEMPO REAL (KAFKA).
            // ESTE PROCESO ES ASÍNCRONO Y PERMITE EVALUAR POSIBLES RIESGOS SIN AFECTAR EL FLUJO PRINCIPAL DE LA TRANSACCIÓN.
            // SI LA VALIDACIÓN DETECTA FRAUDE, SE PODRÍA ACTUALIZAR EL ESTADO DE LA TRANSACCIÓN EN OTRO PUNTO DEL SISTEMA.
            await _transactionAntiFraudService.Validate(bankTransaction);

            return bankTransaction.TransactionExternalId;
        }

        public async Task<BankTransaction> GetTransactionAsync(Guid transactionExternalId)
        {
            return await _transactionRepository.GetByIdAsync(transactionExternalId);
        }
    }
}
