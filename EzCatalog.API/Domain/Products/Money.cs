using System;

namespace EzCatalog.Domain.Products;

public record Money(decimal Amount, Currency Currency)
{
    public Money Add(decimal amount) => new Money(Amount + amount, Currency);

    public Money Multiply(decimal amount) => new Money(Amount * amount, Currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException("When adding Money objects, currencies must match");
        }

        return Add(other.Amount);
    }

    public Money Multiply(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException("When multiplying Money object, currencies must match");
        }

        return Multiply(other.Amount);
    }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}
