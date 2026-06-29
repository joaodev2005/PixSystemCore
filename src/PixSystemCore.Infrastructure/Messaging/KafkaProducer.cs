using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using PixSystemCore.Domain.Events;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishPixRequestedAsync(PixRequestedEvent @event)
    {
        var messageValue = JsonSerializer.Serialize(@event);
        
        await _producer.ProduceAsync("pix-solicitado", new Message<string, string>
        {
            Key = @event.TransactionId.ToString(),
            Value = messageValue
        });

        Console.WriteLine($"[Kafka Producer Real] Evento PixRequested publicado no tópico 'pix-solicitado'. ID: {@event.TransactionId}");
    }

    public async Task PublishPixProcessedAsync(PixProcessedEvent @event)
    {
        var messageValue = JsonSerializer.Serialize(@event);

        await _producer.ProduceAsync("pix-processado", new Message<string, string>
        {
            Key = @event.TransactionId.ToString(),
            Value = messageValue
        });

        Console.WriteLine($"[Kafka Producer Real] Evento PixProcessed publicado no tópico 'pix-processado'. ID: {@event.TransactionId}");
    }
}
