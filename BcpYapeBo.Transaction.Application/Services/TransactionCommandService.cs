using BcpYapeBo.Transaction.Application.Commands;
using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.Enums;
using BcpYapeBo.Transaction.Domain.ValueObjects;
using System.Transactions;

namespace BcpYapeBo.Transaction.Application.Services
{
    public class TransactionCommandService : ITransactionCommandService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionAntiFraudService _transactionAntiFraudService;

        public TransactionCommandService(ITransactionRepository transactionRepository, ITransactionAntiFraudService transactionAntiFraudService)
        {
            _transactionRepository = transactionRepository;
            _transactionAntiFraudService = transactionAntiFraudService;
        }

        public async Task<BankTransaction> Handle(CreateTransactionCommand command)
        {
            // CREA UNA NUEVA INSTANCIA DE LA TRANSACCIÓN BANCARIA CON LOS DATOS PROPORCIONADOS.
            // EL MODELO TIENE REGLAS DE VALIDACION QUE GARANTIZAN LA CONSISTENCIA DE LOS DATOS.
            var bankTransaction = new BankTransaction(
                command.SourceAccountId,
                command.TargetAccountId,
                command.TransferTypeId,
                command.Value
            );

            // GUARDA LA TRANSACCIÓN EN EL REPOSITORIO DE BASE DE DATOS.
            // ESTE PASO ASEGURA LA PERSISTENCIA ANTES DE ENVIARLA AL SISTEMA ANTIFRAUDE.
            await _transactionRepository.SaveAsync(bankTransaction);

            // ENVÍO DE LA TRANSACCIÓN AL SERVICIO DE ANTI-FRAUDE A TRAVÉS DE KAFKA.
            // ESTE PROCESO SE EJECUTA DE FORMA ASÍNCRONA, PERMITIENDO EVALUAR POSIBLES RIESGOS 
            // SIN AFECTAR EL FLUJO PRINCIPAL DE LA TRANSACCIÓN. 
            // SI EL SERVICIO DETECTA FRAUDE, EN OTRO PUNTO DEL SISTEMA SE PUEDE ACTUALIZAR EL ESTADO.
            await _transactionAntiFraudService.Validate(bankTransaction);

            // TODO. ACA SE PUEDEN COLOCAR OTRAS REGLAS DE NEGOCIO ASOCIADOS A LA TRANSACCIÓN.

            // RETORNA LA TRANSACCIÓN, 
            return bankTransaction;
        }

        public async Task UpdateTransactionStatusWithAntiFraudCheckAsync(AntiFraudValidationResult antiFraudValidationResult)
        {
            // OBTIENE LA TRANSACCIÓN DESDE EL REPOSITORIO DE BASE DE DATOS.
            var transaction = await _transactionRepository.GetByIdAsync(antiFraudValidationResult.TransactionExternalId);

            if (transaction != null)
            {
                // ACTUALIZA EL ESTADO DE LA TRANSACCIÓN Y LA RAZÓN DE RECHAZO.
                transaction.MarkAsProcessed(antiFraudValidationResult.Status, antiFraudValidationResult.RejectionReason);
                await _transactionRepository.UpdateAsync(transaction);
            }
        }
    }
}
