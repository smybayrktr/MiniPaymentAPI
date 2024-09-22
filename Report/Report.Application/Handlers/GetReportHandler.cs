using MediatR;
using Report.Application.Interfaces;
using Report.Contracts.DTOs;
using Report.Contracts.Queries;

namespace Report.Application.Handlers;

public class GetReportHandler : IRequestHandler<GetReportQuery, IEnumerable<TransactionReportDto>>
{
    private readonly IReportService _reportService;

    public GetReportHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<TransactionReportDto>> Handle(GetReportQuery query,
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

        return await _reportService.GetReportAsync(query);
    }
}