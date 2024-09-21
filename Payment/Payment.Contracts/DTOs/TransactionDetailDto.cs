using Payment.Domain.Enums;

namespace Payment.Contracts.DTOs;

public class TransactionDetailDto
{
    public Guid DetailId { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
}