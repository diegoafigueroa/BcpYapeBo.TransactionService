namespace BcpYapeBo.Transaction.Domain.Exceptions
{
    /// <summary>
    /// EXCEPCIÓN PARA CUANDO NO SE ENCUENTRA UNA TRANSACCIÓN
    /// </summary>
    public class TransactionNotFoundException : Exception
    {
        /// <summary>
        /// ID DE LA TRANSACCIÓN QUE NO SE ENCONTRÓ
        /// </summary>
        public Guid TransactionId { get; }

        public TransactionNotFoundException(Guid transactionId)
            : base($"La transacción con ID {transactionId} no fue encontrada.")
        {
            TransactionId = transactionId;
        }
    }
}
