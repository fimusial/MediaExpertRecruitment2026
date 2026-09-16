using System;
using EzCatalog.WebAPI.ApiDocumentation;
using EzCatalog.WebAPI.Hypermedia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace EzCatalog.WebAPI;

public static class ServiceCollectionBuilder
{
    public const string OpenApiDocumentName = "v1";
    public const string AllowedCorsOriginsKey = "AllowedCorsOrigins";
    public const string TrustedProxyNetworksKey = "TrustedProxyNetworks";

    public static IServiceCollection AddWebAPI(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddProblemDetails()
            .Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        serviceCollection.AddForwardedHeadersFromProxies(configuration);

        serviceCollection.AddCorsForBrowserClients(configuration);

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

    private static IServiceCollection AddForwardedHeadersFromProxies(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var trustedProxyNetworks = configuration.GetSection(TrustedProxyNetworksKey).Get<string[]>() ?? Array.Empty<string>();

        return serviceCollection.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;

            foreach (var network in trustedProxyNetworks)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });
    }

    private static IServiceCollection AddCorsForBrowserClients(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection(AllowedCorsOriginsKey).Get<string[]>() ?? Array.Empty<string>();

        return serviceCollection.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins.Length == 0)
            {
                return;
            }

            policy
                .WithOrigins(allowedOrigins)
                .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Patch)
                .AllowAnyHeader()
                .WithExposedHeaders(HeaderNames.Location);
        }));
    }
}
