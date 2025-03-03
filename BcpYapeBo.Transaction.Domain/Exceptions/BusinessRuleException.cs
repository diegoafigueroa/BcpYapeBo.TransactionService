namespace BcpYapeBo.Transaction.Domain.Exceptions
{
    /// <summary>
    /// EXCEPCIÓN PARA VIOLACIONES DE REGLAS DE NEGOCIO
    /// </summary>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        /// CÓDIGO DE LA REGLA DE NEGOCIO VIOLADA
        /// </summary>
        public string BusinessRuleCode { get; }

        public BusinessRuleException(string message, string businessRuleCode = null)
            : base(message)
        {
            BusinessRuleCode = businessRuleCode;
        }
    }
}
