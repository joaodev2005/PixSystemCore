using PixSystemCore.Domain.Events;

namespace PixSystemCore.Domain.Interfaces;

public interface IKafkaProducer
{
    Task PublishPixRequestedAsync(PixRequestedEvent @event);
    Task PublishPixProcessedAsync(PixProcessedEvent @event);
}
