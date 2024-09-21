using MediatR;
using Payment.Domain.Entities;

namespace Payment.Contracts.Commands;

public class PayTransactionCommand : IRequest<Transaction>
{
    public string BankId { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderReference { get; set; }
}
