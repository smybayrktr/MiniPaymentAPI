using FluentValidation;
using Payment.Contracts.Queries;

namespace Payment.Application.Validators;

public class SearchPaymentQueryValidator : AbstractValidator<SearchPaymentQuery>
{
    public SearchPaymentQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("StartDate must be less than or equal to EndDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("EndDate must be greater than or equal to StartDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}   