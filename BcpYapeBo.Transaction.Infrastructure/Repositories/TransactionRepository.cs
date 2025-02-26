using BcpYapeBo.Transaction.Application.Ports.Driven;
using BcpYapeBo.Transaction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BcpYapeBo.Transaction.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionDbContext _context;

        public TransactionRepository(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(BankTransaction transaction)
        {
            //await _context.Transactions.AddAsync(transaction);
            //await _context.SaveChangesAsync();
        }

        public async Task<BankTransaction> GetByIdAsync(Guid transactionExternalId)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionExternalId == transactionExternalId);
        }

        public async Task UpdateAsync(BankTransaction bankTransaction)
        {
            _context.Transactions.Update(bankTransaction);
            await _context.SaveChangesAsync();
        }
    }
}
