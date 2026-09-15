using System;

namespace EzCatalog.Application.Contexts;

public interface IOperationContext
{
    Guid OperationId { get; }

    DateTime UtcTimestamp { get; }

    Guid? CorrelationId { get; set; }
}
