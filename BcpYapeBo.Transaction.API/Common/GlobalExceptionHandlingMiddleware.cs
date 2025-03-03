using BcpYapeBo.Transaction.API.DTOs;
using BcpYapeBo.Transaction.Domain.Exceptions;
using Npgsql;
using System;
using System.Net;
using System.Text.Json;

namespace BcpYapeBo.Transaction.API.Common
{
    /// <summary>
    /// MIDDLEWARE GLOBAL PARA MANEJO DE EXCEPCIONES
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            catch (PropertyValidationException ex)
            {
                await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "ErrorDeValidacion");
            }
            catch (BusinessRuleException ex)
            {
                await HandleExceptionAsync(context, ex, HttpStatusCode.UnprocessableEntity, "ViolacionReglaNegocio");
            }
            catch (PostgresException ex)
            {
                _logger.LogError(ex, "Error en la base de datos: {Mensaje}. TraceId: {TraceId}", ex.Message, context.TraceIdentifier);
                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "ErrorPostgreSQL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en la petición {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "ErrorDeServidor");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string errorCode)
        {
            // REGISTRAR LA EXCEPCIÓN COMPLETA EN LOS LOGS PARA DEPURACIÓN INTERNA
            _logger.LogError(exception, "Excepción manejada: {ErrorCode} - {Mensaje}", errorCode, exception.Message);

            // CONFIGURAR LA RESPUESTA HTTP
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // DEFINIR MENSAJES GENÉRICOS SEGÚN EL CÓDIGO DE ESTADO HTTP
            string clientMessage = statusCode switch
            {
                HttpStatusCode.BadRequest => "La solicitud es inválida. Por favor, revise los datos enviados.",
                HttpStatusCode.UnprocessableEntity => "No se puede procesar la solicitud debido a reglas de negocio.",
                HttpStatusCode.InternalServerError => "Ocurrió un error interno en el servidor. Intente nuevamente más tarde.",
                _ => "Ocurrió un error inesperado."
            };

            // CREAR UNA RESPUESTA SEGURA PARA EL CLIENTE
            var response = new ApiErrorResponse(
                context.TraceIdentifier,
                errorCode,
                clientMessage
            );

            // ENVIAR LA RESPUESTA AL CLIENTE
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
