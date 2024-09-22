namespace Report.Contracts.DTOs;

public class TransactionDetailDto
{
    public Guid DetailId { get; set; }
    public TransactionTypeDto TransactionType { get; set; }
    public TransactionStatusDto Status { get; set; }
    public decimal Amount { get; set; }
}