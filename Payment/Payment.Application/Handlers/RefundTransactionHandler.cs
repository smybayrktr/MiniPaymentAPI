using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Handlers;

public class CancelTransactionHandler : IRequestHandler<CancelTransactionCommand, Transaction>
{
    private readonly IPaymentService _paymentService;
    private readonly ITimeZoneService _timeZoneService;

    public CancelTransactionHandler(IPaymentService paymentService, ITimeZoneService timeZoneService)
    {
        _paymentService = paymentService;
        _timeZoneService = timeZoneService;
    }

    public async Task<Transaction> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _paymentService.CancelAsync(request);
        transaction.TransactionDate = _timeZoneService.ConvertUtcToLocalTime(transaction.TransactionDate);

        return transaction;
    }
}
