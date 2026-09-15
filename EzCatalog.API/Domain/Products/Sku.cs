using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using EzCatalog.Domain.Products.Exceptions;

namespace EzCatalog.Domain.Products;

public partial record Sku
{
    private Sku(string value)
    {
        Value = value;
    }

    [GeneratedRegex("^[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SkuPattern();

    public const int MinLength = 4;

    public const int MaxLength = 32;

    public string Value { get; }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out Sku? sku, [NotNullWhen(false)] out string? error)
    {
        sku = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "SKU cannot be empty.";
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length < MinLength || normalized.Length > MaxLength)
        {
            error = $"Normalized SKU must be between {MinLength} and {MaxLength} characters.";
            return false;
        }

        if (!SkuPattern().IsMatch(normalized))
        {
            error = "SKU may contain only letters, digits and single hyphens, and cannot start or end with a hyphen.";
            return false;
        }

        sku = new Sku(normalized);
        error = null;
        return true;
    }

    public static Sku Create(string? value)
    {
        if (!TryCreate(value, out var sku, out var error))
        {
            throw new InvalidSkuException(error);
        }

        return sku;
    }

    public override string ToString() => Value;
}
