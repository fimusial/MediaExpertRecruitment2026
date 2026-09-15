using System;
using System.Linq;
using EzCatalog.Application.Queries;
using EzCatalog.WebAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EzCatalog.WebAPI.Hypermedia;

public sealed class ResourceLinker
{
    private readonly LinkGenerator linkGenerator;
    private readonly IHttpContextAccessor httpContextAccessor;

    public ResourceLinker(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        this.linkGenerator = linkGenerator;
        this.httpContextAccessor = httpContextAccessor;
    }

    public ApiRootResponse ToApiRootResponse(int defaultLimit)
    {
        return new ApiRootResponse(new ApiRootLinks(
            Self: CreateLink(EndpointNames.GetApiRoot, HttpMethods.Get),
            Products: CreateLink(EndpointNames.GetProductsPage, HttpMethods.Get, new { limit = defaultLimit })));
    }

    public ProductResponse ToProductResponse(ProductResult product)
    {
        return new ProductResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.PriceAmount,
            product.PriceCurrency,
            new ProductLinks(
                Self: CreateLink(EndpointNames.GetProduct, HttpMethods.Get, new { id = product.Id }),
                Update: CreateLink(EndpointNames.UpdateProduct, HttpMethods.Patch, new { id = product.Id })));
    }

    public ProductsPageResponse ToProductsPageResponse(ProductsPageResult page, Guid? cursor, int limit)
    {
        var next = page.NextCursor is null
            ? null
            : CreateLink(EndpointNames.GetProductsPage, HttpMethods.Get, new { cursor = page.NextCursor, limit });

        return new ProductsPageResponse(
            page.TotalCount,
            page.NextCursor,
            page.Products.Select(ToProductResponse).ToList(),
            new ProductsPageLinks(
                Self: CreateLink(EndpointNames.GetProductsPage, HttpMethods.Get, new { cursor, limit }),
                First: CreateLink(EndpointNames.GetProductsPage, HttpMethods.Get, new { limit }),
                Next: next,
                Create: CreateLink(EndpointNames.AddProduct, HttpMethods.Post)));
    }

    private Link CreateLink(string endpointName, string method, object? values = null)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Links can only be generated while processing an HTTP request.");

        var href = linkGenerator.GetUriByName(httpContext, endpointName, values)
            ?? throw new InvalidOperationException($"Unable to generate a link to the '{endpointName}' endpoint.");

        return new Link(href, method);
    }
}
