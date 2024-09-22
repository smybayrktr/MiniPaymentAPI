using FluentValidation.TestHelper;
using Payment.Application.Validators;
using Payment.Contracts.Commands;

namespace Payment.Tests.Validators;

public class PayTransactionCommandValidatorTests
{
    private readonly PayTransactionCommandValidator _validator;

    public PayTransactionCommandValidatorTests()
    {
        _validator = new PayTransactionCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_BankId_Is_Empty()
    {
        var model = new PayTransactionCommand { BankId = "", TotalAmount = 100, OrderReference = "Order001" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BankId);
    }

    [Fact]
    public void Should_Have_Error_When_BankId_Exceeds_MaxLength()
    {
        var model = new PayTransactionCommand
            { BankId = new string('A', 51), TotalAmount = 100, OrderReference = "Order001" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BankId)
            .WithErrorMessage("BankId must not exceed 50 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_TotalAmount_Is_Less_Than_Or_Equal_To_Zero()
    {
        var model = new PayTransactionCommand { BankId = "Bank01", TotalAmount = 0, OrderReference = "Order001" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount)
            .WithErrorMessage("TotalAmount must be greater than zero.");
    }

    [Fact]
    public void Should_Have_Error_When_OrderReference_Is_Empty()
    {
        var model = new PayTransactionCommand { BankId = "Bank01", TotalAmount = 100, OrderReference = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.OrderReference);
    }

    [Fact]
    public void Should_Have_Error_When_OrderReference_Exceeds_MaxLength()
    {
        var model = new PayTransactionCommand
            { BankId = "Bank01", TotalAmount = 100, OrderReference = new string('A', 101) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.OrderReference)
            .WithErrorMessage("OrderReference must not exceed 100 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var model = new PayTransactionCommand
        {
            BankId = "Bank01",
            TotalAmount = 100,
            OrderReference = "Order001"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}