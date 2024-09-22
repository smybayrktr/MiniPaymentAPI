using FluentValidation;
using Payment.Contracts.Commands;

namespace Payment.Application.Validators;

public class RefundTransactionCommandValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required.");
    }
}