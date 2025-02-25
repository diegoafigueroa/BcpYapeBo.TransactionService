using BcpYapeBo.Transaction.Domain.Enums;

namespace BcpYapeBo.Transaction.Infrastructure.DTOs
{
    public class AntiFraudValidation
    {
        public Guid TransactionExternalId { get; set; }
        public TransactionStatus Status { get; set; }
        public string RejectionReason { get; set; }
    }
}
