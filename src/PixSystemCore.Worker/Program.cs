using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PixSystemCore.Infrastructure.Data;
using PixSystemCore.Infrastructure.Services;
using StackExchange.Redis;

namespace PixSystemCore.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices((hostContext, services) =>
            {
                string connectionString = "Server=127.0.0.1,1433;Database=PixSystemDb;User Id=sa;Password=DevPass123!;Encrypt=False;TrustServerCertificate=True;";

                services.AddDbContext<PixSystemCoreDbContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect("localhost:6379"));

                services.AddSingleton<RedisService>();

                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = "localhost:9092",
                    GroupId = "pix-worker-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                services.AddSingleton<IConsumer<string, string>>(sp =>
                    new ConsumerBuilder<string, string>(consumerConfig).Build());

                services.AddHostedService<PixWorker>();
            });

            var host = builder.Build();

            Console.WriteLine("=== PixSystemCore.Worker Iniciado ===");
            Console.WriteLine("Escutando o Kafka e pronto para processar transações...");

            await host.RunAsync();
        }
    }
}