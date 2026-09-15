using System;
using EzCatalog.Application.Ports;

namespace EzCatalog.Application.Contexts;

public class OperationContext : IOperationContext
{
    private Guid? correlationId;

    public OperationContext(IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        OperationId = guidProvider.GetNewGuid();
        UtcTimestamp = dateTimeProvider.GetUtcNow();
    }

    public Guid OperationId { get; }

    public DateTime UtcTimestamp { get; }

    public Guid CorrelationId
    {
        get
        {
            if (!correlationId.HasValue)
            {
                correlationId = Guid.NewGuid();
            }

            return correlationId.Value;
        }

        set
        {
            if (correlationId.HasValue)
            {
                throw new InvalidOperationException(
                    $"{nameof(CorrelationId)} has already been set for this instance of {nameof(OperationContext)}");
            }

            correlationId = value;
        }
    }
}
