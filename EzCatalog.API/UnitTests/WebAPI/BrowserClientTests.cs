using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace EzCatalog.UnitTests.WebAPI;

public sealed class BrowserClientTests : IClassFixture<CatalogApiFactory>
{
    private const string AllowedOrigin = "http://localhost:4200";
    private const string DisallowedOrigin = "https://evil.example.com";

    private readonly HttpClient client;

    public BrowserClientTests(CatalogApiFactory factory)
    {
        client = factory
            .WithWebHostBuilder(builder => builder.UseSetting($"{EzCatalog.WebAPI.ServiceCollectionBuilder.AllowedCorsOriginsKey}:0", AllowedOrigin))
            .CreateClient();
    }

    [Fact]
    public async Task Preflight_FromAllowedOrigin_AllowsTheRequest()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Options, new Uri("/products", UriKind.Relative));
        request.Headers.Add(HeaderNames.Origin, AllowedOrigin);
        request.Headers.Add(HeaderNames.AccessControlRequestMethod, HttpMethod.Post.Method);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.GetValues(HeaderNames.AccessControlAllowOrigin).Should().Equal(AllowedOrigin);
        response.Headers.GetValues(HeaderNames.AccessControlAllowMethods).Should().ContainSingle()
            .Which.Should().Contain(HttpMethod.Post.Method);
    }

    [Fact]
    public async Task Preflight_FromDisallowedOrigin_IsNotAllowed()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Options, new Uri("/products", UriKind.Relative));
        request.Headers.Add(HeaderNames.Origin, DisallowedOrigin);
        request.Headers.Add(HeaderNames.AccessControlRequestMethod, HttpMethod.Post.Method);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Headers.Contains(HeaderNames.AccessControlAllowOrigin).Should().BeFalse();
    }

    [Fact]
    public async Task AddProduct_FromAllowedOrigin_ExposesLocationHeaderToTheBrowser()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/products", UriKind.Relative))
        {
            Content = JsonContent.Create(new
            {
                sku = $"CORS-{Guid.NewGuid():N}"[..32],
                name = "Cors",
                priceAmount = 1.99m,
                priceCurrency = "PLN",
            }),
        };
        request.Headers.Add(HeaderNames.Origin, AllowedOrigin);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.GetValues(HeaderNames.AccessControlAllowOrigin).Should().Equal(AllowedOrigin);
        response.Headers.GetValues(HeaderNames.AccessControlExposeHeaders).Should().ContainSingle()
            .Which.Should().Contain(HeaderNames.Location);
    }

    [Fact]
    public async Task ErrorResponse_FromAllowedOrigin_IsStillReadableByTheBrowser()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/products?limit=5", UriKind.Relative));
        request.Headers.Add(HeaderNames.Origin, AllowedOrigin);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Headers.GetValues(HeaderNames.AccessControlAllowOrigin).Should().Equal(AllowedOrigin);
    }

    [Fact]
    public async Task ValidationErrors_AreKeyedByTheFieldNamesTheClientSent()
    {
        // Act
        using var response = await client.PostAsJsonAsync(
            new Uri("/products", UriKind.Relative),
            new { sku = string.Empty, name = string.Empty, priceAmount = 0m, priceCurrency = "pln" },
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonNode>(TestContext.Current.CancellationToken);
        problem!["errors"]!.AsObject().Select(error => error.Key)
            .Should().BeEquivalentTo("sku", "name", "priceAmount", "priceCurrency");
    }
}
