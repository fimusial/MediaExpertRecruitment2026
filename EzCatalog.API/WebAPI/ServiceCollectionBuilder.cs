using EzCatalog.WebAPI.ApiDocumentation;
using EzCatalog.WebAPI.Hypermedia;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EzCatalog.WebAPI;

public static class ServiceCollectionBuilder
{
    public const string OpenApiDocumentName = "v1";

    public static IServiceCollection AddWebAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddProblemDetails()
            .Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        serviceCollection
            .AddHttpContextAccessor()
            .AddScoped<ResourceLinker>();

        serviceCollection.AddOpenApi(OpenApiDocumentName, options =>
        {
            options
                .AddDocumentTransformer(OpenApiTransformers.DescribeDocument)
                .AddSchemaTransformer(OpenApiTransformers.RemoveStringAlternativeFromNumbers)
                .AddSchemaTransformer(OpenApiTransformers.DescribeCurrencyValues);
        });

        return serviceCollection;
    }
}
