using BcpYapeBo.Transaction.API.DTOs;
using BcpYapeBo.Transaction.Application.Ports.Driving;
using BcpYapeBo.Transaction.Application.Services;
using BcpYapeBo.Transaction.Domain;
using BcpYapeBo.Transaction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BcpYapeBo.Transaction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDTO transactionRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var transactionId = await _transactionService.CreateTransactionAsync(
                    sourceAccountId: transactionRequest.SourceAccountId,
                    targetAccountId: transactionRequest.TargetAccountId,
                    transferTypeId: transactionRequest.TransferTypeId,
                    value: transactionRequest.Value
                );

                return Ok(new { TransactionExternalId = transactionId });
            }
            catch (ArgumentException ex)
            {
                // ERRORES DE VALIDACIÓN DE ENTRADA (POR EJEMPLO, TIPO DE TRANSACCIÓN INVÁLIDO)
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // ERRORES DE REGLAS DE NEGOCIO (POR EJEMPLO, TRANSACCIÓN ENTRE LA MISMA CUENTA)
                return UnprocessableEntity(new { error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }

        [HttpGet("{transactionExternalId}")]
        public async Task<IActionResult> GetTransaction(Guid transactionExternalId)
        {
            var transaction = await _transactionService.GetTransactionAsync(transactionExternalId);
            
            if (transaction == null) 
                return NotFound();

            return Ok(new
            {
                transaction.TransactionExternalId,
                transaction.CreatedAt
            });
        }
    }

}