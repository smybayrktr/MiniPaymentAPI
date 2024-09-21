using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Handlers;

public class CancelTransactionHandler : IRequestHandler<CancelTransactionCommand, Transaction>
{
    private readonly IPaymentService _paymentService;

    public CancelTransactionHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Transaction> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.CancelAsync(request);
    }
}
