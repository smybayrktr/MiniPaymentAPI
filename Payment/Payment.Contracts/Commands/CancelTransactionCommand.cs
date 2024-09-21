using MediatR;
using Payment.Domain.Entities;

namespace Payment.Contracts.Commands;

public class CancelTransactionCommand : IRequest<Transaction>
{
    public Guid TransactionId { get; set; }
}
