using FluentValidation.TestHelper;
using Payment.Application.Validators;
using Payment.Contracts.Queries;
using Payment.Domain.Enums;

namespace Payment.Tests.Validators;

public class SearchPaymentQueryValidatorTests
{
    private readonly SearchPaymentQueryValidator _validator;

    public SearchPaymentQueryValidatorTests()
    {
        _validator = new SearchPaymentQueryValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var model = new SearchPaymentQuery
        {
            BankId = "Akbank",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_001",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}