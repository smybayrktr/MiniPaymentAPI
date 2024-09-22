using FluentValidation;
using Payment.Contracts.Commands;

namespace Payment.Application.Validators;

public class PayTransactionCommandValidator : AbstractValidator<PayTransactionCommand>
{
    public PayTransactionCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEmpty().WithMessage("BankId is required.")
            .MaximumLength(50).WithMessage("BankId must not exceed 50 characters.");
            
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");
            
        RuleFor(x => x.OrderReference)
            .NotEmpty().WithMessage("OrderReference is required.")
            .MaximumLength(100).WithMessage("OrderReference must not exceed 100 characters.");
    }
}