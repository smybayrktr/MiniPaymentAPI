using Report.Application.Interfaces;
using Report.Contracts.Queries;
using Report.Domain.Entities;
using Report.Infrastructure.Clients;

namespace Report.Application.Services;

public class ReportService : IReportService
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public ReportService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient;
    }

    public async Task<IEnumerable<Transaction>> GetReportAsync(GetReportQuery query)
    {
        return await _paymentServiceClient.SearchTransactionsAsync(query);
    }
}
