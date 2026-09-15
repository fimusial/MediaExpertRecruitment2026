using System;

namespace EzCatalog.Domain.Products.Exceptions;

public class InvalidSkuException : Exception
{
    public InvalidSkuException()
    {
    }

    public InvalidSkuException(string message)
        : base(message)
    {
    }

    public InvalidSkuException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
