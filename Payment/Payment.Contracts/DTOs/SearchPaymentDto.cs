using Payment.Domain.Enums;

namespace Payment.Contracts.DTOs;

public class SearchPaymentDto
{
    public string? BankId { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? OrderReference { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
