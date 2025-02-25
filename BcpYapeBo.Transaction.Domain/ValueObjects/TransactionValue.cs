namespace BcpYapeBo.Transaction.Domain.ValueObjects
{
    public record TransactionValue(decimal Amount)
    {
        public static TransactionValue Create(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("El monto de la transacción debe ser positivo");

            return new TransactionValue(amount);
        }
    }
}