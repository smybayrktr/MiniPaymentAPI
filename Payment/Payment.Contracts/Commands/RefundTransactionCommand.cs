using MediatR;
using Payment.Domain.Entities;

namespace Payment.Contracts.Commands;

public class RefundTransactionCommand : IRequest<Transaction>
{
    public Guid TransactionId { get; set; }
}
