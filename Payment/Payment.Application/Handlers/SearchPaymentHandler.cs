using MediatR;
using Payment.Application.Interfaces;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;

namespace Payment.Application.Handlers;

public class SearchPaymentHandler : IRequestHandler<SearchPaymentQuery, IEnumerable<TransactionReportDto>>
{
    private readonly ISearchService _searchService;
    private readonly ITimeZoneService _timeZoneService;

    public SearchPaymentHandler(ISearchService searchService, ITimeZoneService timeZoneService)
    {
        _searchService = searchService;
        _timeZoneService = timeZoneService;
    }

    public async Task<IEnumerable<TransactionReportDto>> Handle(SearchPaymentQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StartDate.HasValue && query.StartDate.Value.Kind != DateTimeKind.Utc)
        {
            query.StartDate = DateTime.SpecifyKind(query.StartDate.Value, DateTimeKind.Utc);
        }

        if (query.EndDate.HasValue && query.EndDate.Value.Kind != DateTimeKind.Utc)
        {
            query.EndDate = DateTime.SpecifyKind(query.EndDate.Value, DateTimeKind.Utc);
        }
        
        var reportDtos = await _searchService.SearchAsync(query);

        foreach (var reportDto in reportDtos)
        {
            reportDto.TransactionDate = _timeZoneService.ConvertUtcToLocalTime(reportDto.TransactionDate);
        }

        return reportDtos;
    }
}