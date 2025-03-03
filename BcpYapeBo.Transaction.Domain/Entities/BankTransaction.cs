using System;
using BcpYapeBo.Transaction.Domain.Enums;
using BcpYapeBo.Transaction.Domain.Exceptions;
using BcpYapeBo.Transaction.Domain.ValueObjects;

namespace BcpYapeBo.Transaction.Domain.Entities
{
    public class BankTransaction
    {
        public Guid TransactionExternalId { get; private set; }
        public AccountId SourceAccountId { get; private set; }
        public AccountId TargetAccountId { get; private set; }
        public BankTransactionType Type { get; private set; }
        public TransactionValue Value { get; private set; }
        public BankTransactionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string RejectionReason { get; private set; }
        public int RetryCount { get; private set; }

        // ENTITY FRAMEWORK, SERIALIZADORES, ETC
        public BankTransaction()
        {
        }

        public BankTransaction(
            Guid sourceAccountId,
            Guid targetAccountId,
            int transactionTypeId,
            decimal value)
        {
            // VALIDACION ESPECIFICA PARA 
            if (!Enum.IsDefined(typeof(BankTransactionType), transactionTypeId))
                throw new ArgumentException("El tipo de transacción no es válido.");

            // TODAS LAS DEMAS VALIDACIONES DE ENTRADA LAS REALIZA LOS VALUE OBJECTS

            TransactionExternalId = Guid.NewGuid();
            SourceAccountId = AccountId.Create(sourceAccountId);
            TargetAccountId = AccountId.Create(targetAccountId);
            Type = (BankTransactionType)transactionTypeId;
            Value = TransactionValue.Create(value);
            Status = BankTransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            RetryCount = 0;

            ValidateTransactionRules();
        }

        private void ValidateTransactionRules()
        {
            if (SourceAccountId == TargetAccountId)
                throw new BusinessRuleException("No se puede realizar una transacción entre la misma cuenta.");

            // OTRAS REGLAS DE LA ENTIDAD ! 
        }

        public void MarkAsProcessed(BankTransactionStatus newStatus, string rejectionReason = null)
        {
            Status = newStatus;
            ProcessedAt = DateTime.UtcNow;
            RejectionReason = rejectionReason;
        }

    }
}
