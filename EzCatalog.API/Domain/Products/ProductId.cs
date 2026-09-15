using System;

namespace EzCatalog.Domain.Products;

public record ProductId
{
    private ProductId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString();

    public static ProductId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Guid value for {nameof(ProductId)} cannot be empty.");
        }

        return new ProductId(value);
    }
}
