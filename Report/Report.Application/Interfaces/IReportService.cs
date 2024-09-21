using Report.Contracts.Queries;
using Report.Domain.Entities;

namespace Report.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<Transaction>> GetReportAsync(GetReportQuery query);
}
