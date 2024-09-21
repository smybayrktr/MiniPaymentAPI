using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Handlers;

public class PayTransactionHandler : IRequestHandler<PayTransactionCommand, Transaction>
{
    private readonly IPaymentService _paymentService;

    public PayTransactionHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Transaction> Handle(PayTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.PayAsync(request);
    }
}
