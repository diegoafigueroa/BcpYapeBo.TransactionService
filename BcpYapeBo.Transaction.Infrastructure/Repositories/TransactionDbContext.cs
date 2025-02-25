using BcpYapeBo.Transaction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BcpYapeBo.Transaction.Infrastructure.Repositories
{
    public class TransactionDbContext : DbContext
    {
        public DbSet<BankTransaction> Transactions { get; set; }
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

        public TransactionDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasKey(t => t.TransactionExternalId);
                entity.Property(t => t.TransactionExternalId).ValueGeneratedNever();
                entity.Property(t => t.Type).HasConversion<int>();
                entity.Property(t => t.Status).HasConversion<int>();
                entity.Property(t => t.CreatedAt).IsRequired();
                entity.Property(t => t.ProcessedAt).IsRequired(false);
                entity.Property(t => t.RejectionReason).HasMaxLength(255).IsRequired(false);
                entity.Property(t => t.RetryCount).IsRequired();
            });
        }
    }
}
