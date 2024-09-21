namespace Payment.Contracts.DTOs;

public class PayTransactionDto
{
    public string BankId { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderReference { get; set; }
}
