using EzCatalog.Application.Validators;
using FluentValidation;

namespace EzCatalog.Application.Commands;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public const string PriceIncompleteMessage =
        $"{nameof(UpdateProductCommand.PriceAmount)} and {nameof(UpdateProductCommand.PriceCurrency)} must be provided together.";

    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).MustBeValidProductId();

        RuleFor(command => command.Name)
            .MustBeValidProductName()
            .When(command => command.Name != null);

        RuleFor(command => command.PriceAmount)
            .NotNull()
            .WithMessage(PriceIncompleteMessage)
            .When(command => command.PriceCurrency != null);

        RuleFor(command => command.PriceCurrency)
            .NotNull()
            .WithMessage(PriceIncompleteMessage)
            .When(command => command.PriceAmount != null);

        RuleFor(command => command.PriceAmount)
            .MustBeValidPriceAmountNullable()
            .When(command => command.PriceAmount != null);

        RuleFor(command => command.PriceCurrency)
            .MustBeValidCurrencyNullable()
            .When(command => command.PriceCurrency != null);
    }
}
