using Microsoft.EntityFrameworkCore;
using PixSystemCore.Domain.Entities;
using PixSystemCore.Domain.Interfaces;
using PixSystemCore.Infrastructure.Data;

namespace PixSystemCore.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PixSystemCoreDbContext _context;

    public TransactionRepository(PixSystemCoreDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountAsync(Guid accountId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.TargetAccount)
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking() 
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == key, cancellationToken);
    }
}