using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;

namespace Payment.Application.Handlers;

public class SearchTransactionsHandler : IRequestHandler<SearchPaymentQuery, IEnumerable<TransactionReportDto>>
{
    private readonly ISearchService _searchService;

    public SearchTransactionsHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IEnumerable<TransactionReportDto>> Handle(SearchPaymentQuery request,
        CancellationToken cancellationToken)
    {
        return await _searchService.SearchAsync(request);
    }
}