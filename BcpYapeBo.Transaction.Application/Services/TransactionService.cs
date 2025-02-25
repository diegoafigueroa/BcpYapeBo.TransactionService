using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Application.Ports.Driving;
using BcpYapeBo.Transaction.Domain.Entities;
using BcpYapeBo.Transaction.Domain.ValueObjects;
using System.Transactions;

namespace BcpYapeBo.Transaction.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionAntiFraudService _transactionAntiFraudService;

        public TransactionService(ITransactionRepository transactionRepository, ITransactionAntiFraudService transactionAntiFraudService)
        {
            _transactionRepository = transactionRepository;
            _transactionAntiFraudService = transactionAntiFraudService;
        }

        public async Task<Guid> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
        {
            // CREA UNA NUEVA INSTANCIA DE LA TRANSACCIÓN BANCARIA CON LOS DATOS PROPORCIONADOS.
            // EL MODELO TIENE REGLAS DE VALIDACION QUE GARANTIZAN LA CONSISTENCIA DE LOS DATOS.
            var bankTransaction = new BankTransaction(
                sourceAccountId: sourceAccountId,
                targetAccountId: targetAccountId,
                transactionTypeId: transferTypeId,
                value: value
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

            // RETORNA EL IDENTIFICADOR ÚNICO EXTERNO DE LA TRANSACCIÓN, 
            // QUE PUEDE SER UTILIZADO PARA RASTREO O REFERENCIA EN OTROS PROCESOS DEL SISTEMA.
            return bankTransaction.TransactionExternalId;
        }

        public async Task<BankTransaction> GetTransactionAsync(Guid transactionExternalId)
        {
            return await _transactionRepository.GetByIdAsync(transactionExternalId);
        }
    }
}
