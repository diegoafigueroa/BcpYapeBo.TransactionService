namespace BcpYapeBo.Transaction.Domain.Exceptions
{
    /// <summary>
    /// EXCEPCIÓN PARA ERROR DE VALIDACIÓN DE DATOS
    /// </summary>
    public class PropertyValidationException : Exception
    {
        public PropertyValidationException(string message)
            : base(message)
        {
        }
    }
}
