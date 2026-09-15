using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EzCatalog.Domain.Products;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.WebAPI;

public sealed class OpenApiDocumentTests : IClassFixture<CatalogApiFactory>
{
    private const string BuildTimeDocumentRelativePath = "openapi/ezcatalog-api.json";

    private readonly HttpClient client;

    public OpenApiDocumentTests(CatalogApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task OpenApiDocument_DescribesAllOperations()
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var operationIds = document["paths"]!.AsObject()
            .SelectMany(path => path.Value!.AsObject())
            .Select(operation => operation.Value!["operationId"]!.GetValue<string>());

        operationIds.Should().BeEquivalentTo("GetApiRoot", "GetProduct", "AddProduct", "UpdateProduct", "GetProductsPage");
    }

    [Theory]
    [InlineData("ProductResponse", "priceAmount", "number")]
    [InlineData("ProductsPageResponse", "totalCount", "integer")]
    public async Task OpenApiDocument_DescribesNumbersWithoutStringAlternative(string schemaName, string propertyName, string expectedType)
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var property = document["components"]!["schemas"]![schemaName]!["properties"]![propertyName]!;
        property["type"]!.GetValue<string>().Should().Be(expectedType);
        property["pattern"].Should().BeNull();
    }

    [Fact]
    public async Task OpenApiDocument_DescribesUpdateProductFieldsAsOptional()
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        document["components"]!["schemas"]!["UpdateProductCommand"]!.AsObject().Should().NotContainKey("required");
    }

    [Fact]
    public async Task OpenApiDocument_DescribesInternalServerErrorOnAllOperations()
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var operations = document["paths"]!.AsObject().SelectMany(path => path.Value!.AsObject()).ToList();
        operations.Should().NotBeEmpty();
        operations.Should().OnlyContain(operation =>
            operation.Value!["responses"]!["500"]!["content"]!["application/problem+json"]!["schema"]!["$ref"]!.GetValue<string>()
                == "#/components/schemas/ProblemDetails");
    }

    [Fact]
    public async Task OpenApiDocument_DescribesLocationHeaderOfCreatedProduct()
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var location = document["paths"]!["/products"]!["post"]!["responses"]!["201"]!["headers"]!["Location"]!;
        location["required"]!.GetValue<bool>().Should().BeTrue();
        location["schema"]!["format"]!.GetValue<string>().Should().Be("uri");
    }

    [Theory]
    [InlineData("AddProductCommand")]
    [InlineData("ProductResponse")]
    public async Task OpenApiDocument_DescribesCurrencyValues(string schemaName)
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var currency = document["components"]!["schemas"]![schemaName]!["properties"]!["priceCurrency"]!;
        currency["enum"]!.AsArray().Select(value => value!.GetValue<string>())
            .Should().Equal(Enum.GetNames<Currency>());
    }

    [Fact]
    public async Task OpenApiDocument_DescribesNullableCurrencyValuesIncludingNull()
    {
        // Act
        var document = await GetOpenApiDocumentAsync();

        // Assert
        var currency = document["components"]!["schemas"]!["UpdateProductCommand"]!["properties"]!["priceCurrency"]!;
        currency["enum"]!.AsArray().Select(value => value?.GetValue<string>())
            .Should().Equal([.. Enum.GetNames<Currency>(), null]);
    }

    [Fact]
    public async Task BuildTimeOpenApiDocument_MatchesRuntimeDocument()
    {
        // Arrange
        var runtimeDocument = await GetOpenApiDocumentAsync();
        runtimeDocument.AsObject().Remove("servers");

        // Act
        var buildTimeDocument = JsonNode.Parse(await File.ReadAllTextAsync(FindBuildTimeDocumentPath(), TestContext.Current.CancellationToken))!;

        // Assert
        JsonNode.DeepEquals(buildTimeDocument, runtimeDocument).Should().BeTrue(
            "the build writes {0} from the same configuration the application serves", BuildTimeDocumentRelativePath);
    }

    [Fact]
    public async Task SwaggerUI_IsServed()
    {
        // Act
        using var response = await client.GetAsync(new Uri("/swagger/index.html", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string FindBuildTimeDocumentPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, BuildTimeDocumentRelativePath);
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException($"Build-time OpenAPI document '{BuildTimeDocumentRelativePath}' was not found.");
    }

    private async Task<JsonNode> GetOpenApiDocumentAsync()
    {
        using var response = await client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonNode>(TestContext.Current.CancellationToken);
        return json!;
    }
}
