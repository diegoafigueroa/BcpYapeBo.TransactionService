using BcpYapeBo.Transaction.Domain.Entities;

namespace BcpYapeBo.Transaction.Application.Ports.Driving
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value);
        Task<BankTransaction> GetTransactionAsync(Guid transactionExternalId);
    }
}
