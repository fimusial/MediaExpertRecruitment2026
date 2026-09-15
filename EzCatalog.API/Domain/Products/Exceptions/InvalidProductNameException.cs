using System;

namespace EzCatalog.Domain.Products.Exceptions;

public class InvalidProductNameException : Exception
{
    public InvalidProductNameException()
    {
    }

    public InvalidProductNameException(string message)
        : base(message)
    {
    }

    public InvalidProductNameException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
