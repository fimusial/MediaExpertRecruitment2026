using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EzCatalog.Domain.Products.Exceptions;

namespace EzCatalog.Domain.Products;

public record ProductName
{
    private ProductName(string value)
    {
        Value = value;
    }

    public const int MinLength = 1;

    public const int MaxLength = 500;

    public string Value { get; }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out ProductName? name, [NotNullWhen(false)] out string? error)
    {
        name = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Product name cannot be empty.";
            return false;
        }

        if (value.Any(char.IsControl))
        {
            error = "Product name cannot contain control characters.";
            return false;
        }

        var normalized = value.Trim();
        if (normalized.Length < MinLength || normalized.Length > MaxLength)
        {
            error = $"Normalized product name must be between {MinLength} and {MaxLength} characters.";
            return false;
        }

        name = new ProductName(normalized);
        error = null;
        return true;
    }

    public static ProductName Create(string? value)
    {
        if (!TryCreate(value, out var name, out var error))
        {
            throw new InvalidProductNameException(error);
        }

        return name;
    }

    public override string ToString() => Value;
}
