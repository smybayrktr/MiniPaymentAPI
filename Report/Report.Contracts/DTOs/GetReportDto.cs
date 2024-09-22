namespace Report.Contracts.DTOs;

public class GetReportDto
{
    public string? BankId { get; set; }
    public TransactionStatusDto? Status { get; set; }
    public string? OrderReference { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
