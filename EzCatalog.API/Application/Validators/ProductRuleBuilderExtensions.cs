using System;
using System.Globalization;
using EzCatalog.Domain.Products;
using FluentValidation;

namespace EzCatalog.Application.Validators;

public static class ProductRuleBuilderExtensions
{
    public const string PriceNotPositiveMessage = "Product price must be positive.";

    public static readonly string PriceTooLargeMessage =
        $"Product price must not exceed {Product.PriceMaxAmount.ToString(CultureInfo.InvariantCulture)}.";

    public static readonly string PriceTooPreciseMessage =
        $"Product price must have at most {Product.PriceMaxDecimalPlaces} decimal places.";

    public static IRuleBuilderOptionsConditions<T, string?> MustBeValidSku<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            if (!Sku.TryCreate(value, out _, out var error))
            {
                context.AddFailure(error);
            }
        });
    }

    public static IRuleBuilderOptionsConditions<T, string?> MustBeValidProductName<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            if (!ProductName.TryCreate(value, out _, out var error))
            {
                context.AddFailure(error);
            }
        });
    }

    public static IRuleBuilderOptions<T, decimal?> MustBeValidPriceAmountNullable<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .GreaterThan(0.0m)
            .WithMessage(PriceNotPositiveMessage)
            .LessThanOrEqualTo(Product.PriceMaxAmount)
            .WithMessage(PriceTooLargeMessage)
            .Must(amount => amount is null || HasAllowedDecimalPlaces(amount.Value))
            .WithMessage(PriceTooPreciseMessage);
    }

    public static IRuleBuilderOptions<T, decimal> MustBeValidPriceAmount<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0.0m)
            .WithMessage(PriceNotPositiveMessage)
            .LessThanOrEqualTo(Product.PriceMaxAmount)
            .WithMessage(PriceTooLargeMessage)
            .Must(HasAllowedDecimalPlaces)
            .WithMessage(PriceTooPreciseMessage);
    }

    public static IRuleBuilderOptions<T, string?> MustBeValidCurrencyNullable<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .IsEnumName(typeof(Currency))
            .WithMessage($"Currency must be one of: {string.Join(", ", Enum.GetNames<Currency>())}.");
    }

    public static IRuleBuilderOptions<T, string> MustBeValidCurrency<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .IsEnumName(typeof(Currency))
            .WithMessage($"Currency must be one of: {string.Join(", ", Enum.GetNames<Currency>())}.");
    }

    public static IRuleBuilderOptions<T, Guid> MustBeValidProductId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage($"{nameof(ProductId)} cannot be empty.");
    }

    private static bool HasAllowedDecimalPlaces(decimal amount) =>
        decimal.Round(amount, Product.PriceMaxDecimalPlaces) == amount;
}
