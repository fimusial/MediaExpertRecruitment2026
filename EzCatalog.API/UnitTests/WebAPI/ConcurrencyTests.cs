using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.WebAPI;

public sealed class ConcurrencyTests : IClassFixture<CatalogApiFactory>
{
    private const int ConcurrentRequests = 200;

    private readonly HttpClient client;

    public ConcurrencyTests(CatalogApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentRequests_AreAllServedSuccessfully()
    {
        // Arrange
        var id = await GetFirstProductIdAsync();

        // Act
        var responses = await WhenAllAsync(Enumerable.Range(0, ConcurrentRequests).Select(index => (index % 4) switch
        {
            0 => client.GetAsync(new Uri("/products?limit=100", UriKind.Relative), TestContext.Current.CancellationToken),
            1 => client.GetAsync(new Uri($"/products/{id}", UriKind.Relative), TestContext.Current.CancellationToken),
            2 => client.PostAsJsonAsync(
                new Uri("/products", UriKind.Relative),
                new { sku = $"CONCURRENT-{Guid.NewGuid():N}"[..32], name = "Concurrent", priceAmount = 1.99m, priceCurrency = "PLN" },
                TestContext.Current.CancellationToken),
            _ => client.PatchAsJsonAsync(
                new Uri($"/products/{id}", UriKind.Relative),
                new { name = $"Concurrent {index}" },
                TestContext.Current.CancellationToken),
        }));

        // Assert
        responses.Should().OnlyContain(statusCode => statusCode < HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConcurrentCreationsOfTheSameSku_CreateTheProductOnlyOnce()
    {
        // Arrange
        var sku = $"RACE-{Guid.NewGuid():N}"[..32];

        // Act
        var responses = await WhenAllAsync(Enumerable.Range(0, ConcurrentRequests).Select(_ => client.PostAsJsonAsync(
            new Uri("/products", UriKind.Relative),
            new { sku, name = "Race", priceAmount = 1.99m, priceCurrency = "PLN" },
            TestContext.Current.CancellationToken)));

        // Assert
        responses.Should().ContainSingle(statusCode => statusCode == HttpStatusCode.Created);
        responses.Should().OnlyContain(statusCode => statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.Conflict);
    }

    private static async Task<IReadOnlyCollection<HttpStatusCode>> WhenAllAsync(IEnumerable<Task<HttpResponseMessage>> requests)
    {
        var responses = await Task.WhenAll(requests);

        try
        {
            return responses.Select(response => response.StatusCode).ToList();
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private async Task<Guid> GetFirstProductIdAsync()
    {
        var page = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/products?limit=10", UriKind.Relative),
            TestContext.Current.CancellationToken);

        return page!["products"]![0]!["id"]!.GetValue<Guid>();
    }
}
