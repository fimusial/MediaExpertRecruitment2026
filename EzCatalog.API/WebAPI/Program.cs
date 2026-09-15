using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using EzCatalog.Application;
using EzCatalog.Application.Commands;
using EzCatalog.Application.Queries;
using EzCatalog.Infrastructure;
using EzCatalog.WebAPI;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonConsole(formatterOptions =>
{
    formatterOptions.IncludeScopes = true;
    formatterOptions.UseUtcTimestamp = true;
    formatterOptions.JsonWriterOptions = new JsonWriterOptions { Indented = true };
});

builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddWebAPI();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMiddleware<OperationContextLoggerScopeMiddleware>();

app.MapPost(
    "/products",
    async (
        [FromBody] AddProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken) =>
{
    await mediator.Send(command, cancellationToken);
    return Results.Created();
});

app.MapPatch(
    "/products/{id}",
    async (
        [FromRoute] Guid id,
        [FromBody] UpdateProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken) =>
{
    // TODO: separate API model
    command = new UpdateProductCommand(id, command.Name, command.PriceAmount, command.PriceCurrency);
    await mediator.Send(command, cancellationToken);
    return Results.NoContent();
});

app.MapGet(
    "/products",
    async (
        [FromBody] GetProductsPageQuery query,
        IMediator mediator,
        CancellationToken cancellationToken) =>
{
    var results = await mediator.Send(query, cancellationToken);
    return Results.Ok(results);
});

app.Run();
