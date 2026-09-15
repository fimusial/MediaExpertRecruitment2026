using EzCatalog.Application.Validators;
using FluentValidation;

namespace EzCatalog.Application.Queries;

public class GetProductsPageQueryValidator : AbstractValidator<GetProductsPageQuery>
{
    public GetProductsPageQueryValidator()
    {
        RuleFor(query => query.Cursor!.Value)
            .MustBeValidProductId()
            .When(query => query.Cursor != null)
            .OverridePropertyName(nameof(GetProductsPageQuery.Cursor));

        RuleFor(query => query.Limit)
            .InclusiveBetween(10, 100);
    }
}
