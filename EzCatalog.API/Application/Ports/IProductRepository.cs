using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Domain.Products;

namespace EzCatalog.Application.Ports;

public interface IProductRepository
{
    Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task UpdateAsync(Product product, CancellationToken cancellationToken);
}
