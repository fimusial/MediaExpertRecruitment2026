using System;
using System.Threading.Tasks;
using EzCatalog.Application.Logging;
using Microsoft.AspNetCore.Http;

namespace EzCatalog.WebAPI;

public class OperationContextLoggerScopeMiddleware
{
    private readonly RequestDelegate next;

    public OperationContextLoggerScopeMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
    {
        using var loggerScope = serviceProvider.CreateOperationContextLoggerScope();
        await next(context);
    }
}
