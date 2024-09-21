using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Report.Domain.Entities;
using Report.Contracts.DTOs;

namespace Report.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ReportController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a report of transactions based on the provided search criteria.
    /// </summary>
    /// <param name="request">The parameters used to filter and generate the report.</param>
    /// <returns>A list of transactions that match the specified criteria.</returns>
    /// <response code="200">Returns the generated report containing the matching transactions.</response>
    /// <response code="400">If the request parameters are invalid or missing.</response>
    /// <response code="500">If an internal server error occurs while generating the report.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetReport([FromQuery] GetReportDto request)
    {
        if (request == null)
        {
            return BadRequest("Report request parameters are required.");
        }

        var query = _mapper.Map<Contracts.Queries.GetReportQuery>(request);

        try
        {
            var report = await _mediator.Send(query);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while generating the report: {ex.Message}");
        }
    }
}
