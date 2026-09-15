using EzCatalog.Application.Validators;
using FluentValidation;

namespace EzCatalog.Application.Queries;

public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(query => query.Id).MustBeValidProductId();
    }
}
