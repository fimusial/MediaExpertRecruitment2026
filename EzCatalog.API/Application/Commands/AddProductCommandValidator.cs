using EzCatalog.Application.Validators;
using FluentValidation;

namespace EzCatalog.Application.Commands;

public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator()
    {
        RuleFor(command => command.Sku).MustBeValidSku();
        RuleFor(command => command.Name).MustBeValidProductName();
        RuleFor(command => command.PriceAmount).MustBeValidPriceAmount();
        RuleFor(command => command.PriceCurrency).MustBeValidCurrency();
    }
}
