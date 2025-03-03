namespace BcpYapeBo.Transaction.API.DTOs
{
    public class TransactionResponseDTO
    {
        public Guid TransactionExternalId { get; }
        public DateTime CreatedAt { get; }

        public TransactionResponseDTO(Guid transactionExternalId, DateTime createdAt)
        {
            TransactionExternalId = transactionExternalId;
            CreatedAt = createdAt;
        }
    }
}
