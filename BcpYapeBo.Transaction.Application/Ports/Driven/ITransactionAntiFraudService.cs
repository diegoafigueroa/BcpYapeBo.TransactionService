using BcpYapeBo.Transaction.Domain.Entities;

namespace BcpYapeBo.Transaction.Application.Ports.Driven
{
    public interface ITransactionAntiFraudService
    {
        Task Validate(BankTransaction message);
    }
}
