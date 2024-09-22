using FluentValidation;
using Report.Contracts.Queries;

namespace Report.Application.Validators;

public class GetReportQueryValidator : AbstractValidator<GetReportQuery>
{
    public GetReportQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("StartDate must be less than or equal to EndDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("EndDate must be greater than or equal to StartDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}