using MediatR;
using Report.Contracts.DTOs;

namespace Report.Contracts.Queries;

public class GetReportQuery : IRequest<IEnumerable<TransactionReportDto>>
{
    public string? BankId { get; set; }
    public TransactionStatusDto? Status { get; set; }
    public string? OrderReference { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
