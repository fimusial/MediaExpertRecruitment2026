using EzCatalog.Application.Validators;
using FluentValidation;

namespace EzCatalog.Application.Commands;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).MustBeValidProductId();

        RuleFor(command => command.Name)
            .MustBeValidProductName()
            .When(command => command.Name != null);

        RuleFor(command => command.PriceAmount)
            .MustBeValidPriceAmountNullable()
            .When(command => command.PriceCurrency != null)
            .OverridePropertyName(nameof(UpdateProductCommand.PriceAmount));

        RuleFor(command => command.PriceCurrency)
            .MustBeValidCurrencyNullable()
            .When(command => command.PriceAmount != null);
    }
}
