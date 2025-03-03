using BcpYapeBo.Transaction.Domain.Entities;

namespace BcpYapeBo.Transaction.Application.Queries
{
    public interface ITransactionQueryService
    {
        Task<BankTransaction> GetTransactionByIdAsync(Guid transactionExternalId);
    }
}
