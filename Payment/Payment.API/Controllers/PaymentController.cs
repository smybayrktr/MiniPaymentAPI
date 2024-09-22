using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PaymentController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Initiates a new payment transaction.
    /// </summary>
    /// <param name="request">The payment details required to process the transaction.</param>
    /// <returns>The details of the processed transaction.</returns>
    /// <response code="200">Returns the created transaction.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("pay")]
    [ProducesResponseType(typeof(Transaction), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Pay([FromBody] PayTransactionDto request)
    {
        var command = _mapper.Map<PayTransactionCommand>(request);

        var transaction = await _mediator.Send(command);

        return Ok(transaction);
    }

    /// <summary>
    /// Cancels an existing transaction based on the provided transaction ID.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction to cancel.</param>
    /// <returns>The updated details of the canceled transaction.</returns>
    /// <response code="200">Returns the canceled transaction.</response>
    /// <response code="404">If the transaction is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("cancel/{transactionId}")]
    [ProducesResponseType(typeof(Transaction), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Cancel(Guid transactionId)
    {
        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = await _mediator.Send(command);

        return Ok(transaction);
    }

    /// <summary>
    /// Processes a refund for an existing transaction based on the provided transaction ID.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction to refund.</param>
    /// <returns>The updated details of the refunded transaction.</returns>
    /// <response code="200">Returns the refunded transaction.</response>
    /// <response code="404">If the transaction is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("refund/{transactionId}")]
    [ProducesResponseType(typeof(Transaction), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Refund(Guid transactionId)
    {
        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = await _mediator.Send(command);

        return Ok(transaction);
    }

    /// <summary>
    /// Searches for transactions based on the provided search criteria.
    /// </summary>
    /// <param name="searchDto">The search parameters to filter transactions.</param>
    /// <returns>A list of transactions matching the search criteria.</returns>
    /// <response code="200">Returns the list of matching transactions.</response>
    /// <response code="400">If the search parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<TransactionReportDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Search([FromQuery] SearchPaymentDto searchDto)
    {
        var query = _mapper.Map<SearchPaymentQuery>(searchDto);

        var transactions = await _mediator.Send(query);
        
        return Ok(transactions);
    }
}