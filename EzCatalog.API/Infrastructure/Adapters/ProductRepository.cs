using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using EzCatalog.Domain.Products;

namespace EzCatalog.Infrastructure.Adapters;

public class ProductRepository : IProductRepository
{
    public Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken)
    {
        // TODO: Entity Framework repository
        return null!;
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        // TODO: Entity Framework repository
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        // TODO: Entity Framework repository
        return Task.CompletedTask;
    }
}
