using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Handlers;

public class RefundTransactionHandler : IRequestHandler<RefundTransactionCommand, Transaction>
{
    private readonly IPaymentService _paymentService;

    public RefundTransactionHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Transaction> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.RefundAsync(request);
    }
}
