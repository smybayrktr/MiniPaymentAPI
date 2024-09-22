using FluentValidation.TestHelper;
using Payment.Application.Validators;
using Payment.Contracts.Commands;

namespace Payment.Tests.Validators;

public class CancelTransactionCommandValidatorTests
{
    private readonly CancelTransactionCommandValidator _validator;

    public CancelTransactionCommandValidatorTests()
    {
        _validator = new CancelTransactionCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TransactionId_Is_Empty()
    {
        var model = new CancelTransactionCommand { TransactionId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TransactionId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TransactionId_Is_Not_Empty()
    {
        var model = new CancelTransactionCommand { TransactionId = Guid.NewGuid() };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.TransactionId);
    }
}