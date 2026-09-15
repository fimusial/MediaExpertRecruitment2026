using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using EzCatalog.Application;
using EzCatalog.Application.Commands;
using EzCatalog.Application.Queries;
using EzCatalog.Infrastructure;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.WebAPI;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapGet(
    "/products/{id}",
    async (
        [FromRoute] Guid id,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var product = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return Results.Ok(product);
    })
    .WithName("GetProduct");

app.MapPost(
    "/products",
    async (
        [FromBody] AddProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var id = await mediator.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute("GetProduct", new { id });
    })
    .WithName("AddProduct");

app.MapPatch(
    "/products/{id}",
    async (
        [FromRoute] Guid id,
        [FromBody] EzCatalog.WebAPI.DTOs.UpdateProductCommand dto,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var command = new UpdateProductCommand(id, dto.Name, dto.PriceAmount, dto.PriceCurrency);
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    })
    .WithName("UpdateProduct");

app.MapGet(
    "/products",
    async (
        [FromQuery] Guid? cursor,
        [FromQuery] int? limit,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var results = await mediator.Send(new GetProductsPageQuery(cursor, limit ?? GetProductsPageQuery.DefaultLimit), cancellationToken);
        return Results.Ok(results);
    })
    .WithName("GetProductsPage");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Ctrl+C pressed.");
    e.Cancel = true;
    cts.Cancel();
};

if (app.Configuration.GetValue<bool>("SeedTestDataOnStartup"))
{
    var catalogDbContext = app.Services.GetRequiredService<CatalogDbContext>();
    await catalogDbContext.Database.EnsureCreatedAsync(cts.Token);
}

await app.RunAsync(cts.Token);
