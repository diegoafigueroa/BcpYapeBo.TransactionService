using BcpYapeBo.Transaction.Domain.Entities;

namespace BcpYapeBo.Transaction.Application.Ports.Driven
{
    public interface ITransactionRepository
    {
        Task SaveAsync(BankTransaction bankTransaction);
        Task<BankTransaction> GetByIdAsync(Guid transactionExternalId);
    }
}
