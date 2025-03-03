namespace BcpYapeBo.Transaction.Application.Commands
{
    public class CreateTransactionCommand
    {
        public Guid SourceAccountId { get; }
        public Guid TargetAccountId { get; }
        public int TransferTypeId { get; }
        public decimal Value { get; }

        public CreateTransactionCommand(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
        {
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            TransferTypeId = transferTypeId;
            Value = value;
        }
    }
}
