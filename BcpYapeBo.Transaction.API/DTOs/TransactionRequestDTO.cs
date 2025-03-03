using BcpYapeBo.Transaction.Application.Commands;
using System.ComponentModel.DataAnnotations;

namespace BcpYapeBo.Transaction.API.DTOs
{
    public class TransactionRequestDTO
    {
        [Required]
        public Guid SourceAccountId { get; set; }

        [Required]
        public Guid TargetAccountId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El TransferTypeId debe ser mayor que 0.")]
        public int TransferTypeId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El Value debe ser mayor que 0.")]
        public decimal Value { get; set; }

        // PARA SERIALIZADORES
        public TransactionRequestDTO() { }

        public CreateTransactionCommand ToCommand()
        {
            return new CreateTransactionCommand(
                this.SourceAccountId,
                this.TargetAccountId,
                this.TransferTypeId,
                this.Value
            );
        }
    }
}
