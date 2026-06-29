using PixSystemCore.Domain.Entities;

namespace PixSystemCore.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByAccountAsync(Guid accountId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
}
