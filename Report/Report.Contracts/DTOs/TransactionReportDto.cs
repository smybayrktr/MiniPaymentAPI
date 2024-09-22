namespace Report.Contracts.DTOs;

public class TransactionReportDto
{
    public Guid TransactionId { get; set; }
    public string BankId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal NetAmount { get; set; }
    public TransactionStatusDto Status { get; set; }
    public string OrderReference { get; set; }
    public DateTime TransactionDate { get; set; }
    public List<TransactionDetailDto> TransactionDetails { get; set; }
}