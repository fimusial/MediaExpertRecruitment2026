using System;

namespace EzCatalog.Domain.Products;

public record ProductId
{
    private ProductId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public override string ToString()
    {
        return $"{nameof(ProductId)}: {Value}";
    }

    public static ProductId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Guid value for ProductId cannot be empty");
        }

        if (value.Variant != 7)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Guid value for ProductId must be version 7");
        }

        return new ProductId(value);
    }
}
