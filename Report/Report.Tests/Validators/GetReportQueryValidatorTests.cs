using FluentValidation.TestHelper;
using Report.Application.Validators;
using Report.Contracts.Queries;

namespace Report.Tests.Validators;

public class GetReportQueryValidatorTests
{
    private readonly GetReportQueryValidator _validator;

    public GetReportQueryValidatorTests()
    {
        _validator = new GetReportQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_Greater_Than_EndDate()
    {
        var model = new GetReportQuery
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("StartDate must be less than or equal to EndDate.");
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Less_Than_StartDate()
    {
        var model = new GetReportQuery
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("EndDate must be greater than or equal to StartDate.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_StartDate_Is_Less_Than_Or_Equal_To_EndDate()
    {
        var model = new GetReportQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Should_Not_Have_Error_When_StartDate_Or_EndDate_Is_Null()
    {
        var model1 = new GetReportQuery
        {
            StartDate = null,
            EndDate = DateTime.UtcNow
        };
        var result1 = _validator.TestValidate(model1);
        result1.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result1.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        var model2 = new GetReportQuery
        {
            StartDate = DateTime.UtcNow,
            EndDate = null
        };
        var result2 = _validator.TestValidate(model2);
        result2.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result2.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}