using PixSystemCore.Domain.Enums;

namespace PixSystemCore.Domain.Events;

public class PixProcessedEvent
{
    public Guid TransactionId { get; init; }
    public TransactionStatus Status { get; init; } 
    public DateTime ProcessedAt { get; init; }
}
