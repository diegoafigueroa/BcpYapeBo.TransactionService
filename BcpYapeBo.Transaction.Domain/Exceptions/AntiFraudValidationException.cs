namespace BcpYapeBo.Transaction.Domain.Exceptions
{
    public class AntiFraudValidationException : Exception
    {
        public AntiFraudValidationException(string message) 
            : base(message)
        {
        }

        public AntiFraudValidationException(string message, Exception ex)
        : base(message, ex)
        {
        }
    }
}
