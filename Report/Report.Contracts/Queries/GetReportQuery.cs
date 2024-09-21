using MediatR;
using Report.Domain.Entities;
using Report.Domain.Enums;

namespace Report.Contracts.Queries;

public class GetReportQuery : IRequest<IEnumerable<Transaction>>
{
    public string? BankId { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? OrderReference { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
