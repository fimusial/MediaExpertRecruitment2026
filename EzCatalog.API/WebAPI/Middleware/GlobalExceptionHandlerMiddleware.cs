using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EzCatalog.Application.Contexts;
using EzCatalog.Application.Exceptions;
using EzCatalog.Domain.Products.Exceptions;
using EzCatalog.Infrastructure.Exceptions;
using EzCatalog.WebAPI.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EzCatalog.WebAPI;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;
    private readonly IProblemDetailsService problemDetailsService;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.next = next;
        this.problemDetailsService = problemDetailsService;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var operationContext = context.RequestServices.GetService<IOperationContext>();

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client is gone, so there is nobody to send a response to.
            logger.LogRequestAborted(
                context.Request.Method,
                context.Request.Path,
                operationContext?.OperationId,
                operationContext?.UtcTimestamp,
                operationContext?.CorrelationId);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                // Status code and headers are already sent; the only option is to let the server abort the response.
                logger.LogExceptionAfterResponseStarted(
                    exception,
                    context.Request.Method,
                    context.Request.Path,
                    operationContext?.OperationId,
                    operationContext?.UtcTimestamp,
                    operationContext?.CorrelationId);
                throw;
            }

            await HandleExceptionAsync(context, exception, operationContext);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new HttpValidationProblemDetails(
                validationException.Errors
                    .GroupBy(error => JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName))
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
            },
            BadHttpRequestException badHttpRequestException => Create(badHttpRequestException.StatusCode, exception.Message),
            InvalidSkuException or InvalidProductNameException or InvalidPriceException => Create(StatusCodes.Status400BadRequest, exception.Message),
            NotFoundException => Create(StatusCodes.Status404NotFound, exception.Message),
            CreateFailedException => Create(StatusCodes.Status409Conflict, exception.Message),
            UpdateFailedException => Create(StatusCodes.Status409Conflict, exception.Message),
            _ => Create(StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        static ProblemDetails Create(int status, string detail) => new ProblemDetails { Status = status, Detail = detail };
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IOperationContext? operationContext)
    {
        var problemDetails = CreateProblemDetails(exception);
        var statusCode = problemDetails.Status!.Value;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogUnhandledException(
                exception,
                context.Request.Method,
                context.Request.Path,
                operationContext?.OperationId,
                operationContext?.UtcTimestamp,
                operationContext?.CorrelationId);
        }
        else
        {
            logger.LogClientError(
                context.Request.Method,
                context.Request.Path,
                statusCode,
                exception.GetType().Name,
                exception.Message,
                operationContext?.OperationId,
                operationContext?.UtcTimestamp,
                operationContext?.CorrelationId);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }
}
