using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string BankId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal NetAmount { get; set; }
    public TransactionStatus Status { get; set; }
    public string OrderReference { get; set; }
    public DateTime TransactionDate { get; set; }
}
