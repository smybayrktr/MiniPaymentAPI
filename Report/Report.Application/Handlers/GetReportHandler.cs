using MediatR;
using Report.Application.Interfaces;
using Report.Contracts.Queries;
using Report.Domain.Entities;

namespace Report.Application.Handlers;

public class GetReportHandler : IRequestHandler<GetReportQuery, IEnumerable<Transaction>>
{
    private readonly IReportService _reportService;

    public GetReportHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<Transaction>> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetReportAsync(request);
    }
}
