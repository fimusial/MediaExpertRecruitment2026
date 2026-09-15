using System;
using Microsoft.Extensions.Logging;

namespace EzCatalog.Application.Logging;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handler running: {handler}")]
    public static partial void LogHandlerRunning(this ILogger logger, string handler);

    [LoggerMessage(Level = LogLevel.Error, Message = "exception occurred")]
    public static partial void LogException(this ILogger logger, Exception ex);
}
