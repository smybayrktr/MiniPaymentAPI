using Report.Contracts.DTOs;
using Report.Contracts.Queries;

namespace Report.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<TransactionReportDto>> GetReportAsync(GetReportQuery query);
}
