using System.Text.Json;

namespace BcpYapeBo.Transaction.API.Common
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error inesperado.");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorResponse = new
            {
                Message = "Ocurrió un error interno en el servidor. Por favor, intenta de nuevo más tarde.",
                RequestId = context.TraceIdentifier
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
