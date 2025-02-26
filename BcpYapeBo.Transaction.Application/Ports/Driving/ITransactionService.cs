using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.Enums;

namespace BcpYapeBo.Transaction.Application.Ports.Driving
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value);
        Task UpdateTransactionStatusWithAntiFraudCheckAsync(AntiFraudValidationResult antiFraudValidationResult);
        Task<BankTransaction> GetTransactionAsync(Guid transactionExternalId);
    }
}
