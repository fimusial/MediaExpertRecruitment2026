using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Queries;

namespace EzCatalog.Application.Ports;

public interface IProductQueryService
{
    Task<ProductsPageResult> GetProductsPageAsync(Guid? cursor, int limit, CancellationToken cancellationToken);
}
