using Microsoft.EntityFrameworkCore;
using PixSystemCore.Domain.Entities;

namespace PixSystemCore.Infrastructure.Data;

public class PixSystemCoreDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public PixSystemCoreDbContext(DbContextOptions<PixSystemCoreDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PixKey).IsUnique();
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.BlockedBalance).HasPrecision(18, 2);
            entity.Property(e => e.HolderName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PixKey).HasMaxLength(100).IsRequired();
            entity.Ignore(e => e.AvailableBalance);
            entity.Ignore(e => e.IsActive);
            entity.Ignore(e => e.KeyType);
            entity.Ignore(e => e.Transactions); 
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdempotencyKey);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);

            entity.HasOne(e => e.SourceAccount)
                .WithMany() 
                .HasForeignKey(e => e.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetAccount)
                .WithMany() 
                .HasForeignKey(e => e.TargetAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
