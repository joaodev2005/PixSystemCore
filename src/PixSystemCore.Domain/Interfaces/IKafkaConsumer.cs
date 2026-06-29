namespace PixSystemCore.Domain.Interfaces;

public interface IKafkaConsumer
{
    Task ConsumeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken);
}
