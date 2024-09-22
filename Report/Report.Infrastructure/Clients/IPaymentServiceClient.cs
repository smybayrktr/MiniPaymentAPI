using Report.Contracts.DTOs;
using Report.Contracts.Queries;

namespace Report.Infrastructure.Clients;

public interface IPaymentServiceClient
{
    Task<IEnumerable<TransactionReportDto>> SearchTransactionsAsync(GetReportQuery query);
}