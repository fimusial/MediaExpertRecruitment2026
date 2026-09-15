using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Exceptions;
using EzCatalog.Application.Ports;
using EzCatalog.Domain.Products;
using MediatR;

namespace EzCatalog.Application.Queries;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductResult>
{
    private readonly IProductRepository repository;

    public GetProductQueryHandler(IProductRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ProductResult> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(ProductId.Create(query.Id), cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"{nameof(Product)} with Id {query.Id} was not found.");
        }

        return ProductResult.FromProduct(product);
    }
}
