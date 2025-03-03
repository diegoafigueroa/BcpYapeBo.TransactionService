using BcpYapeBo.Transaction.API.DTOs;
using BcpYapeBo.Transaction.API.Filters;
using BcpYapeBo.Transaction.Application.Commands;
using BcpYapeBo.Transaction.Application.Queries;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.Domain;
using BcpYapeBo.Transaction.Domain.Exceptions;
using BcpYapeBo.Transaction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace BcpYapeBo.Transaction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionCommandService _commandService;
        private readonly ITransactionQueryService _queryService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionCommandService commandService,
            ITransactionQueryService queryService,
            ILogger<TransactionsController> logger)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una nueva transacción bancaria
        /// </summary>
        /// <param name="transactionRequestDTO">Detalles de la transacción</param>
        /// <returns>ID de la transacción creada</returns>
        /// <response code="201">Devuelve el ID de la nueva transacción creada</response>
        /// <response code="400">Si la solicitud no es válida</response>
        /// <response code="422">Si la transacción no puede ser procesada debido a reglas de negocio</response>
        /// <response code="500">Si ocurre un error inesperado</response>
        [HttpPost("CreateTransaction")]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ValidateModelState]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDTO transactionRequestDTO)
        {
            _logger.LogInformation("Creando transacción entre cuentas {SourceAccountId} y {TargetAccountId}",
                   transactionRequestDTO.SourceAccountId, transactionRequestDTO.TargetAccountId);

            // CONVERTIR EL DTO DE LA TRANSACCIÓN A UN COMANDO
            var command = transactionRequestDTO.ToCommand();

            // EJECUTAR EL COMANDO PARA CREAR LA TRANSACCIÓN
            var bankTransaction = await _commandService.Handle(command);
            _logger.LogInformation("Transacción {TransactionId} creada exitosamente", bankTransaction);

            // DEVOLVER UNA RESPUESTA 201 CREATED
            return CreatedAtAction(nameof(GetTransaction),
                new { transactionExternalId = bankTransaction },
                new TransactionResponseDTO(bankTransaction.TransactionExternalId, bankTransaction.CreatedAt));
        }

        /// <summary>
        /// Recupera una transacción por su ID externo
        /// </summary>
        /// <param name="transactionExternalId">El ID externo de la transacción</param>
        /// <returns>Detalles de la transacción</returns>
        /// <response code="200">Devuelve los detalles de la transacción</response>
        /// <response code="400">Si el ID de transacción es vacío o inválido</response>
        /// <response code="404">Si la transacción no puede ser encontrada</response>
        /// <response code="500">Si ocurre un error inesperado</response>
        [HttpGet("{transactionExternalId}")]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTransaction([Required] Guid transactionExternalId)
        {
            _logger.LogInformation("Recuperando transacción {TransactionId}", transactionExternalId);

            // VALIDAR QUE EL ID DE TRANSACCIÓN NO ESTÉ VACÍO
            if (transactionExternalId == Guid.Empty)
            {
                _logger.LogWarning("Se recibió una solicitud con transactionExternalId vacío.");

                return BadRequest(new ApiErrorResponse(
                    HttpContext.TraceIdentifier,
                    "ParametroInvalido",
                    "El ID de transacción no es válido."));
            }

            // RECUPERAR LA TRANSACCIÓN POR SU ID EXTERNO
            var transaction = await _queryService.GetTransactionByIdAsync(transactionExternalId);

            // SI SE ENCUENTRA VACIO, DEVOLVER UNA RESPUESTA 404 NOT FOUND
            if (transaction == null)
            {
                _logger.LogWarning("Transacción {TransactionId} no encontrada", transactionExternalId);
                    
                return NotFound(new ApiErrorResponse(HttpContext.TraceIdentifier, "RecursoNoEncontrado", "La transacción solicitada no se encuentra en el sistema."));
            }

            _logger.LogInformation("Transacción {TransactionId} recuperada exitosamente", transactionExternalId);

            // DEVOLVER LOS DETALLES MINIMOS DE LA TRANSACCIÓN
            var transactionDTO = new TransactionResponseDTO(transaction.TransactionExternalId, transaction.CreatedAt);

            // DEVOLVER UNA RESPUESTA 200 OK
            return Ok(transactionDTO);
        }
    }
}