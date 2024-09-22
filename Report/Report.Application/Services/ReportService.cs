using Report.Application.Interfaces;
using Report.Contracts.DTOs;
using Report.Contracts.Queries;
using Report.Infrastructure.Clients;

namespace Report.Application.Services;

public class ReportService : IReportService
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public ReportService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient;
    }

    public async Task<IEnumerable<TransactionReportDto>> GetReportAsync(GetReportQuery query)
    {
        return await _paymentServiceClient.SearchTransactionsAsync(query);
    }
}
