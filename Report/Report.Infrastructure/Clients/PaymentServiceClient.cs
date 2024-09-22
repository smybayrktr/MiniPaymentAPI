using Report.Contracts.Queries;
using System.Net.Http.Json;
using Report.Contracts.DTOs;

namespace Report.Infrastructure.Clients;

public class PaymentServiceClient : IPaymentServiceClient
{
    private readonly HttpClient _httpClient;

    public PaymentServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<TransactionReportDto>> SearchTransactionsAsync(GetReportQuery query)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(query.BankId))
            queryParams.Add($"bankId={query.BankId}");
        if (query.Status.HasValue)
            queryParams.Add($"status={query.Status.Value}");
        if (!string.IsNullOrEmpty(query.OrderReference))
            queryParams.Add($"orderReference={query.OrderReference}");
        if (query.StartDate.HasValue)
            queryParams.Add($"startDate={query.StartDate.Value:yyyy-MM-dd}");
        if (query.EndDate.HasValue)
            queryParams.Add($"endDate={query.EndDate.Value:yyyy-MM-dd}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        var response = await _httpClient.GetAsync($"/api/payment/search{queryString}");
        response.EnsureSuccessStatusCode();

        var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<TransactionReportDto>>();
        return transactions;
    }
}
