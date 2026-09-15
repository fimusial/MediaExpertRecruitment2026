using System;

namespace EzCatalog.Domain.Products.Exceptions;

public class InvalidPriceException : Exception
{
    public InvalidPriceException()
    {
    }

    public InvalidPriceException(string message)
        : base(message)
    {
    }

    public InvalidPriceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
