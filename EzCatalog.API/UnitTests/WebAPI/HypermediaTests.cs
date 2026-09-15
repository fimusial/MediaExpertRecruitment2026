using System;
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

public sealed class HypermediaTests : IClassFixture<CatalogApiFactory>
{
    private const int Limit = 10;

    private readonly HttpClient client;

    public HypermediaTests(CatalogApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetApiRoot_ReturnsSelfAndProductsLinks()
    {
        // Act
        var root = await GetJsonAsync(RelativeUri("/"));

        // Assert
        AssertLink(root["_links"]!["self"], AbsoluteUri("/"), HttpMethod.Get);
        AssertLink(root["_links"]!["products"], AbsoluteUri("/products?limit=20"), HttpMethod.Get);
    }

    [Fact]
    public async Task GetApiRoot_FollowingProductsLink_ReturnsProductsPage()
    {
        // Arrange
        var root = await GetJsonAsync(RelativeUri("/"));

        // Act
        var page = await GetJsonAsync(HrefOf(root["_links"]!["products"]));

        // Assert
        page["products"]!.AsArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductsPage_KeepsExistingFieldsAndAddsLinks()
    {
        // Act
        var page = await GetJsonAsync(RelativeUri($"/products?limit={Limit}"));

        // Assert
        page.AsObject().Select(property => property.Key)
            .Should().Equal("totalCount", "nextCursor", "products", "_links");
        page["products"]![0]!.AsObject().Select(property => property.Key)
            .Should().Equal("id", "sku", "name", "priceAmount", "priceCurrency", "_links");
    }

    [Fact]
    public async Task GetProductsPage_WithoutCursor_ReturnsPageLinks()
    {
        // Act
        var page = await GetJsonAsync(RelativeUri($"/products?limit={Limit}"));

        // Assert
        var links = page["_links"]!;
        var nextCursor = page["nextCursor"]!.GetValue<Guid>();
        AssertLink(links["self"], AbsoluteUri($"/products?limit={Limit}"), HttpMethod.Get);
        AssertLink(links["first"], AbsoluteUri($"/products?limit={Limit}"), HttpMethod.Get);
        AssertLink(links["next"], AbsoluteUri($"/products?cursor={nextCursor}&limit={Limit}"), HttpMethod.Get);
        AssertLink(links["create"], AbsoluteUri("/products"), HttpMethod.Post);
    }

    [Fact]
    public async Task GetProductsPage_WithoutLimit_UsesDefaultLimitInLinks()
    {
        // Act
        var page = await GetJsonAsync(RelativeUri("/products"));

        // Assert
        AssertLink(page["_links"]!["self"], AbsoluteUri("/products?limit=20"), HttpMethod.Get);
    }

    [Fact]
    public async Task GetProductsPage_FollowingNextLink_ReturnsFollowingPage()
    {
        // Arrange
        var firstPage = await GetJsonAsync(RelativeUri($"/products?limit={Limit}"));
        var nextHref = HrefOf(firstPage["_links"]!["next"]);

        // Act
        var secondPage = await GetJsonAsync(nextHref);

        // Assert
        AssertLink(secondPage["_links"]!["self"], nextHref, HttpMethod.Get);
        var lastIdOfFirstPage = firstPage["products"]!.AsArray()[^1]!["id"]!.GetValue<Guid>();
        secondPage["products"]!.AsArray()
            .Should().HaveCount(Limit)
            .And.OnlyContain(product => product!["id"]!.GetValue<Guid>() < lastIdOfFirstPage);
    }

    [Fact]
    public async Task GetProductsPage_OnLastPage_ReturnsNullNextLink()
    {
        // Arrange
        var cursorBeforeAllProducts = new Guid("00000000-0000-0000-0000-000000000001");

        // Act
        var page = await GetJsonAsync(RelativeUri($"/products?cursor={cursorBeforeAllProducts}&limit={Limit}"));

        // Assert
        page["nextCursor"].Should().BeNull();
        page["_links"]!.AsObject().Should().ContainKey("next");
        page["_links"]!["next"].Should().BeNull();
    }

    [Fact]
    public async Task GetProductsPage_FollowingProductSelfLink_ReturnsSameProduct()
    {
        // Arrange
        var page = await GetJsonAsync(RelativeUri($"/products?limit={Limit}"));
        var listedProduct = page["products"]![0]!;

        // Act
        var product = await GetJsonAsync(HrefOf(listedProduct["_links"]!["self"]));

        // Assert
        JsonNode.DeepEquals(product, listedProduct).Should().BeTrue();
    }

    [Fact]
    public async Task AddProduct_FollowingLocationAndUpdateLink_UpdatesProduct()
    {
        // Arrange
        var sku = $"HATEOAS-{Guid.NewGuid():N}"[..Sku.MaxLength];
        using var createResponse = await client.PostAsJsonAsync(
            RelativeUri("/products"),
            new { sku, name = "Original name", priceAmount = 99.99m, priceCurrency = "PLN" },
            TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = createResponse.Headers.Location!;

        var product = await GetJsonAsync(location);
        AssertLink(product["_links"]!["self"], location, HttpMethod.Get);
        AssertLink(product["_links"]!["update"], location, HttpMethod.Patch);

        // Act
        using var updateResponse = await client.PatchAsJsonAsync(
            HrefOf(product["_links"]!["update"]),
            new { name = "Updated name" },
            TestContext.Current.CancellationToken);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedProduct = await GetJsonAsync(HrefOf(product["_links"]!["self"]));
        updatedProduct["name"]!.GetValue<string>().Should().Be("Updated name");
        updatedProduct["priceAmount"]!.GetValue<decimal>().Should().Be(99.99m);
    }

    private static Uri RelativeUri(string uri) => new Uri(uri, UriKind.Relative);

    private static Uri HrefOf(JsonNode? link) => new Uri(link!["href"]!.GetValue<string>());

    private static void AssertLink(JsonNode? link, Uri expectedHref, HttpMethod expectedMethod)
    {
        link.Should().NotBeNull();
        HrefOf(link).Should().Be(expectedHref);
        link!["method"]!.GetValue<string>().Should().Be(expectedMethod.Method);
    }

    private Uri AbsoluteUri(string relativeUri) => new Uri(client.BaseAddress!, relativeUri);

    private async Task<JsonNode> GetJsonAsync(Uri uri)
    {
        using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonNode>(TestContext.Current.CancellationToken);
        return json!;
    }
}
