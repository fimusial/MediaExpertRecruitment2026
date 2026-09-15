using Microsoft.Extensions.Logging;

namespace EzCatalog.Application.Logging;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handler running: {handler}")]
    public static partial void LogHandlerRunning(this ILogger logger, string handler);
}
