using MediatR;
using Payment.Contracts.DTOs;
using Payment.Domain.Enums;

namespace Payment.Contracts.Queries;

public class SearchPaymentQuery : IRequest<IEnumerable<TransactionReportDto>>
{
    public string? BankId { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? OrderReference { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
