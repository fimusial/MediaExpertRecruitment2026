using System;
using System.Collections.Generic;

namespace EzCatalog.Application.Queries;

public record ProductsPageResult(int TotalCount, Guid? NextCursor, IEnumerable<ProductResult> products)
{
}
