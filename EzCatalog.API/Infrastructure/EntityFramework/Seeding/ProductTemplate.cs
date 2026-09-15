using System.Collections.Generic;

namespace EzCatalog.Infrastructure.EntityFramework.Seeding;

public record ProductTemplate(
    string SkuPrefix,
    string Category,
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> Models,
    IReadOnlyList<string> Variants,
    int MinPrice,
    int MaxPrice);
