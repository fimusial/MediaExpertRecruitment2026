using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EzCatalog.WebAPI.Logging;

public static partial class LogMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unhandled exception while processing {method} {path}, OperationId: {operationId}, OperationUtcTimestamp: {operationUtcTimestamp}, CorrelationId: {correlationId}")]
    public static partial void LogUnhandledException(
        this ILogger logger,
        Exception exception,
        string method,
        PathString path,
        Guid? operationId = null,
        DateTime? operationUtcTimestamp = null,
        Guid? correlationId = null);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Request {method} {path} failed with status {statusCode}: {exceptionType}: {reason}, OperationId: {operationId}, OperationUtcTimestamp: {operationUtcTimestamp}, CorrelationId: {correlationId}")]
    public static partial void LogClientError(
        this ILogger logger,
        string method,
        PathString path,
        int statusCode,
        string exceptionType,
        string reason,
        Guid? operationId = null,
        DateTime? operationUtcTimestamp = null,
        Guid? correlationId = null);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Request {method} {path} was aborted by the client, OperationId: {operationId}, OperationUtcTimestamp: {operationUtcTimestamp}, CorrelationId: {correlationId}")]
    public static partial void LogRequestAborted(
        this ILogger logger,
        string method,
        PathString path,
        Guid? operationId = null,
        DateTime? operationUtcTimestamp = null,
        Guid? correlationId = null);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Exception occurred after the response started for {method} {path}; an error response cannot be written, OperationId: {operationId}, OperationUtcTimestamp: {operationUtcTimestamp}, CorrelationId: {correlationId}")]
    public static partial void LogExceptionAfterResponseStarted(
        this ILogger logger,
        Exception exception,
        string method,
        PathString path,
        Guid? operationId = null,
        DateTime? operationUtcTimestamp = null,
        Guid? correlationId = null);
}
