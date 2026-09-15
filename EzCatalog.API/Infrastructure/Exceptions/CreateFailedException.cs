using System;

namespace EzCatalog.Infrastructure.Exceptions;

public class CreateFailedException : Exception
{
    public CreateFailedException()
    {
    }

    public CreateFailedException(string message)
        : base(message)
    {
    }

    public CreateFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
