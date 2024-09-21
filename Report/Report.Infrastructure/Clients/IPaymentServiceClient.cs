using Report.Contracts.Queries;
using Report.Domain.Entities;

namespace Report.Infrastructure.Clients;

public interface IPaymentServiceClient
{
    Task<IEnumerable<Transaction>> SearchTransactionsAsync(GetReportQuery query);
}
