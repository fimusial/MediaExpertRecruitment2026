using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Domain.Products;
using EzCatalog.WebAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EzCatalog.WebAPI.ApiDocumentation;

public static class OpenApiTransformers
{
    public static Task DescribeDocument(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "EzCatalog API";
        document.Info.Version = ServiceCollectionBuilder.OpenApiDocumentName;
        document.Info.Description = "REST API serving a catalog of products. " +
            "Responses are hypermedia-driven: follow the `_links` of each resource instead of building URIs by hand.";

        return Task.CompletedTask;
    }

    // The web JSON defaults tolerate numbers sent as strings, which the generator documents as "integer | string".
    // Numbers are always written as JSON numbers, so the stricter contract is documented to keep generated clients typed properly.
    public static Task RemoveStringAlternativeFromNumbers(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type is { } type
            && (type.HasFlag(JsonSchemaType.Integer) || type.HasFlag(JsonSchemaType.Number))
            && type.HasFlag(JsonSchemaType.String))
        {
            schema.Type = type & ~JsonSchemaType.String;
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }

    public static Task DescribeCurrencyValues(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonPropertyInfo?.AttributeProvider is not MemberInfo { Name: nameof(ProductResponse.PriceCurrency) }
            || schema.Type is not { } type
            || !type.HasFlag(JsonSchemaType.String))
        {
            return Task.CompletedTask;
        }

        var values = Enum.GetNames<Currency>().Select(name => (JsonNode)JsonValue.Create(name)).ToList();
        if (type.HasFlag(JsonSchemaType.Null))
        {
            values.Add(JsonNullSentinel.JsonNull);
        }

        schema.Enum = values;
        return Task.CompletedTask;
    }

    public static Task DescribeLocationHeader(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Responses?.GetValueOrDefault(StatusCodes.Status201Created.ToString()) is OpenApiResponse response)
        {
            response.Headers ??= new Dictionary<string, IOpenApiHeader>();
            response.Headers["Location"] = new OpenApiHeader
            {
                Description = "URI of the created resource.",
                Required = true,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
            };
        }

        return Task.CompletedTask;
    }
}
