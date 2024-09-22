using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Handlers;

public class PayTransactionHandler : IRequestHandler<PayTransactionCommand, Transaction>
{
    private readonly IPaymentService _paymentService;
    private readonly ITimeZoneService _timeZoneService;

    public PayTransactionHandler(IPaymentService paymentService, ITimeZoneService timeZoneService)
    {
        _paymentService = paymentService;
        _timeZoneService = timeZoneService;
    }

    public async Task<Transaction> Handle(PayTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _paymentService.PayAsync(request);
        transaction.TransactionDate = _timeZoneService.ConvertUtcToLocalTime(transaction.TransactionDate);
        
        return transaction;
    }
}