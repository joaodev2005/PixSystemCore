using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PixSystemCore.Domain.Interfaces;
using PixSystemCore.Infrastructure.Repositories;
using PixSystemCore.Infrastructure.Services;
using StackExchange.Redis;

namespace PixSystemCore.Infrastructure;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IRedisService, RedisService>();

        return services;
    }
}
