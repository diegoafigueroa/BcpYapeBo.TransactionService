using BcpYapeBo.Transaction.API.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BcpYapeBo.Transaction.API.Filters
{
    /// <summary>
    /// FILTRO QUE VALIDA AUTOMÁTICAMENTE EL ESTADO DEL MODELO Y DEVUELVE UNA RESPUESTA DE ERROR ESTANDARIZADA
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var key in context.ModelState.Keys)
                {
                    if (context.ModelState[key].Errors.Count > 0)
                    {
                        errors[key] = context.ModelState[key].Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray();
                    }
                }

                var errorResponse = new ApiErrorResponse(
                    context.HttpContext.TraceIdentifier,
                    "ErrorDeValidacion",
                    "Los datos enviados no son válidos.",
                    errors);

                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }
}
