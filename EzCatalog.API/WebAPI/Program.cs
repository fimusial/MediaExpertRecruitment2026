using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using EzCatalog.Application;
using EzCatalog.Application.Commands;
using EzCatalog.Application.Queries;
using EzCatalog.Infrastructure;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.WebAPI;
using EzCatalog.WebAPI.ApiDocumentation;
using EzCatalog.WebAPI.Hypermedia;
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

if (app.Configuration.GetValue<bool>("ExposeApiDocumentation"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/openapi/{EzCatalog.WebAPI.ServiceCollectionBuilder.OpenApiDocumentName}.json", "EzCatalog API");
        options.DocumentTitle = "EzCatalog API";
    });
}

app.MapGet(
    "/",
    (ResourceLinker linker) => TypedResults.Ok(linker.ToApiRootResponse(GetProductsPageQuery.DefaultLimit)))
    .WithName(EndpointNames.GetApiRoot)
    .WithTags("Root")
    .WithSummary("Get the API entry point")
    .WithDescription("Returns links to the top-level resources, so clients only need to know this single URI.")
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet(
    "/products/{id}",
    async (
        [FromRoute] Guid id,
        IMediator mediator,
        ResourceLinker linker,
        CancellationToken cancellationToken) =>
    {
        var product = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return TypedResults.Ok(linker.ToProductResponse(product));
    })
    .WithName(EndpointNames.GetProduct)
    .WithTags("Products")
    .WithSummary("Get a product")
    .WithDescription("Returns a single product together with links to the actions available on it.")
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapPost(
    "/products",
    async (
        [FromBody] AddProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var id = await mediator.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute(EndpointNames.GetProduct, new { id });
    })
    .WithName(EndpointNames.AddProduct)
    .WithTags("Products")
    .WithSummary("Create a product")
    .WithDescription(
        "Creates a new product. The SKU is normalized to upper case and must be unique. " +
        "The URI of the created product is returned in the `Location` header.")
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .AddOpenApiOperationTransformer(OpenApiTransformers.DescribeLocationHeader);

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
        return TypedResults.NoContent();
    })
    .WithName(EndpointNames.UpdateProduct)
    .WithTags("Products")
    .WithSummary("Update a product")
    .WithDescription(
        "Partially updates a product. Omitted (or null) fields are left unchanged, but at least one change must be provided. " +
        "`priceAmount` and `priceCurrency` must be provided together.")
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet(
    "/products",
    async (
        [FromQuery, Description("Cursor of the page to fetch, taken from `nextCursor` of the previous page. Omit to fetch the first page.")] Guid? cursor,
        [FromQuery, Description("Maximum number of products on the page, between 10 and 100."), DefaultValue(GetProductsPageQuery.DefaultLimit)] int? limit,
        IMediator mediator,
        ResourceLinker linker,
        CancellationToken cancellationToken) =>
    {
        var effectiveLimit = limit ?? GetProductsPageQuery.DefaultLimit;
        var results = await mediator.Send(new GetProductsPageQuery(cursor, effectiveLimit), cancellationToken);
        return TypedResults.Ok(linker.ToProductsPageResponse(results, cursor, effectiveLimit));
    })
    .WithName(EndpointNames.GetProductsPage)
    .WithTags("Products")
    .WithSummary("Get a page of products")
    .WithDescription("Returns products ordered from newest to oldest, using cursor-based pagination. Follow `_links.next` to fetch the next page.")
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Ctrl+C pressed.");
    e.Cancel = true;
    cts.Cancel();
};

var isGeneratingOpenApiDocument = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

if (app.Configuration.GetValue<bool>("SeedTestDataOnStartup") && !isGeneratingOpenApiDocument)
{
    var catalogDbContext = app.Services.GetRequiredService<CatalogDbContext>();
    await catalogDbContext.Database.EnsureCreatedAsync(cts.Token);
}

await app.RunAsync(cts.Token);
