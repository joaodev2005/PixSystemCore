using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T> GetIdempotentResponseAsync<T>(string key)
    {
        var value = await _database.StringGetAsync($"idempotency:{key}");
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task CacheIdempotentResponseAsync<T>(string key, T resposta, int expirationMinutes = 5)
    {
        var json = JsonSerializer.Serialize(resposta);
        await _database.StringSetAsync($"idempotency:{key}", json, TimeSpan.FromMinutes(expirationMinutes));
    }

    public async Task<bool> AcquireLockAsync(string key, decimal amount)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();

        var adquirido = await _database.LockTakeAsync(lockKey, lockValue, TimeSpan.FromSeconds(30));

        if (adquirido)
        {
            await _database.StringSetAsync($"{lockKey}:value", amount.ToString(), TimeSpan.FromSeconds(30));
        }

        return adquirido;
    }

    public async Task ReleaseLockAsync(string key)
    {
        var lockKey = $"lock:{key}";
        await _database.KeyDeleteAsync(lockKey);
        await _database.KeyDeleteAsync($"{lockKey}:value");
    }
}
