using FluentValidation;
using Payment.Contracts.Commands;

namespace Payment.Application.Validators;

public class CancelTransactionCommandValidator : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required.");
    }
}