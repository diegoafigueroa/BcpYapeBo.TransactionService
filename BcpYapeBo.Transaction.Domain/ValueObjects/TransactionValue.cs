using BcpYapeBo.Transaction.Domain.Exceptions;

namespace BcpYapeBo.Transaction.Domain.ValueObjects
{
    public record TransactionValue(decimal Amount)
    {
        public static TransactionValue Create(decimal amount)
        {
            if (amount <= 0)
                throw new PropertyValidationException("El monto de la transacción debe ser positivo");

            return new TransactionValue(amount);
        }
    }
}