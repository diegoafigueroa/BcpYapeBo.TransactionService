using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.Enums;

namespace BcpYapeBo.Transaction.Application.Commands
{
    public interface ITransactionCommandService
    {
        Task<BankTransaction> Handle(CreateTransactionCommand command);
        Task UpdateTransactionStatusWithAntiFraudCheckAsync(AntiFraudValidationResult antiFraudValidationResult);
    }
}
