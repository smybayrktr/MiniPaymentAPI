using Payment.Application.Interfaces;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;
using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public class SearchService: ISearchService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionDetailRepository _transactionDetailRepository;

    public SearchService(ITransactionRepository transactionRepository, ITransactionDetailRepository transactionDetailRepository)
    {
        _transactionRepository = transactionRepository;
        _transactionDetailRepository = transactionDetailRepository;
    }

    public async Task<IEnumerable<TransactionReportDto>> SearchAsync(SearchPaymentQuery query)
    {
        var transactions = await _transactionRepository.GetTransactionsByQuery(query);

        if (!transactions.Any())
            return Enumerable.Empty<TransactionReportDto>();

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var transactionDetails = await _transactionDetailRepository.GetByTransactionIdsAsync(transactionIds);

        var detailsGrouped = transactionDetails.GroupBy(td => td.TransactionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var reportDtos = transactions.Select(t => new TransactionReportDto
        {
            TransactionId = t.Id,
            BankId = t.BankId,
            TotalAmount = t.TotalAmount,
            NetAmount = t.NetAmount,
            Status = t.Status,
            OrderReference = t.OrderReference,
            TransactionDate = t.TransactionDate,
            TransactionDetails = detailsGrouped.ContainsKey(t.Id) ? detailsGrouped[t.Id].Select(d => new TransactionDetailDto
            {
                DetailId = d.Id,
                TransactionType = d.TransactionType,
                Status = d.Status,
                Amount = d.Amount
            }).ToList() : new List<TransactionDetailDto>()
        }).ToList();

        return reportDtos;
    }
}