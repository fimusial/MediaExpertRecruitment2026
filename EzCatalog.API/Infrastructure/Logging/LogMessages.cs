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
        string? transactionId);

    [LoggerMessage(Level = LogLevel.Error, Message = "exception occurred")]
    public static partial void LogException(this ILogger logger, Exception ex);
}
