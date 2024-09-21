using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;

namespace Payment.Application.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<TransactionReportDto>> SearchAsync(SearchPaymentQuery query);
}