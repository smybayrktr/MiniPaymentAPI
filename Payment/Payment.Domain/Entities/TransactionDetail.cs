using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public class TransactionDetail
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public TransactionType TransactionType { get; set; }
    public Enums.TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
}
