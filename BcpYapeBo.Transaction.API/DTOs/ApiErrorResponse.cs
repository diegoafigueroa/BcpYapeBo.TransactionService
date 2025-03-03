namespace BcpYapeBo.Transaction.API.DTOs
{
    /// <summary>
    /// Respuesta estándar para errores de API
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Identificador de seguimiento para correlacionar con logs
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Código de error específico
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Mensaje de error legible para el usuario
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Errores de validación específicos por campo
        /// </summary>
        public Dictionary<string, string[]> ValidationErrors { get; set; }

        /// <summary>
        /// Fecha y hora del error
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiErrorResponse(string traceId, string errorCode, string message, Dictionary<string, string[]> validationErrors = null)
        {
            TraceId = traceId;
            ErrorCode = errorCode;
            Message = message;
            ValidationErrors = validationErrors;
        }
    }
}