using Payment.Contracts.Commands;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<Transaction> PayAsync(PayTransactionCommand command);
    Task<Transaction> CancelAsync(CancelTransactionCommand command);
    Task<Transaction> RefundAsync(RefundTransactionCommand command);
}
