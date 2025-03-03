namespace BcpYapeBo.Transaction.Domain.Exceptions
{
    /// <summary>
    /// EXCEPCIÓN PARA ERRORES DE VALIDACIÓN DE DATOS
    /// </summary>
    public class PropertiesValidationsException : Exception
    {
        /// <summary>
        /// DICCIONARIO DE ERRORES DE VALIDACIÓN POR CAMPO
        /// </summary>
        public Dictionary<string, string[]> ValidationErrors { get; }

        public PropertiesValidationsException(string message, Dictionary<string, string[]> validationErrors)
            : base(message)
        {
            ValidationErrors = validationErrors;
        }
    }
}
