namespace PixSystemCore.Domain.Interfaces;

public interface IRedisService
{
    Task<T?> GetIdempotentResponseAsync<T>(string key);
    Task CacheIdempotentResponseAsync<T>(string key, T response, int expirationMinutes = 5);
    Task<bool> AcquireLockAsync(string key, decimal amount);
    Task ReleaseLockAsync(string key);
}
