using System;
using Microsoft.Extensions.Logging;

namespace EzCatalog.Infrastructure.Logging;

public static partial class LogMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "UnitOfWork step performed: {step}, transactionId: {transactionId}")]
    public static partial void LogUnitOfWorkStep(
        this ILogger logger,
        string step,
        Guid? transactionId);
}
