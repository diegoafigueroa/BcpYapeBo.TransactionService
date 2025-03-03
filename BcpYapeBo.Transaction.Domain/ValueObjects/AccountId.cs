using BcpYapeBo.Transaction.Domain.Exceptions;

namespace BcpYapeBo.Transaction.Domain.ValueObjects
{
    public record AccountId(Guid Value)
    {
        public static AccountId Create(Guid value)
        {
            if (value == Guid.Empty)
                throw new PropertyValidationException("El ID de cuenta no puede estar vacío");

            return new AccountId(value);
        }
    }
}