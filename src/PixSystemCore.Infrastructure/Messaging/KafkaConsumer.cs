using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Infrastructure.Messaging;

public class KafkaConsumer : IKafkaConsumer
{
    public Task ConsumeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Kafka Consumer] Escutando o tópico: {topic}");
        return Task.CompletedTask;
    }
}
