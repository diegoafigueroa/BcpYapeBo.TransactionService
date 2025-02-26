using BcpYapeBo.Transaction.Domain.Enums;

namespace BcpYapeBo.Transaction.Domain.Entities
{
    public class AntiFraudValidationResult
    {
        public Guid TransactionExternalId { get; set; }
        public BankTransactionStatus Status { get; set; }
        public string RejectionReason { get; set; }
    }
}
