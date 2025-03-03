using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Queries;
using BcpYapeBo.Transaction.Domain.Entities;

namespace BcpYapeBo.Transaction.Application.Services
{
    public class TransactionQueryService : ITransactionQueryService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionQueryService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<BankTransaction> GetTransactionByIdAsync(Guid transactionExternalId)
        {
            // SE OBTIENE LA TRANSACCIÓN POR SU IDENTIFICADOR EXTERNO
            var transaction = await _transactionRepository.GetByIdAsync(transactionExternalId);

            // SI NO SE ENCUENTRA LA TRANSACCIÓN, SE RETORNA NULL
            if (transaction == null) 
                return null;

            return transaction;
        }
    }
}
