using Payment.Contracts.Commands;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IBankService
{
    Task<Transaction> PayAsync(PayTransactionCommand command);
    Task<Transaction> CancelAsync(Guid transactionId);
    Task<Transaction> RefundAsync(Guid transactionId);
}
