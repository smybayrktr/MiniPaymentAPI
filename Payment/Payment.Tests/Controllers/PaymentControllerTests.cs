using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Payment.API.Controllers;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new PaymentController(_mediatorMock.Object, _mapperMock.Object);
    }

    #region Pay Tests

    [Fact]
    public async Task Pay_Should_Return_Ok_With_Transaction_When_Request_Is_Valid()
    {
        // Arrange
        var payDto = new PayTransactionDto
        {
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 100m,
            OrderReference = "ORDER123"
        };

        var command = new PayTransactionCommand
        {
            BankId = payDto.BankId,
            TotalAmount = payDto.TotalAmount,
            OrderReference = payDto.OrderReference
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            BankId = payDto.BankId,
            TotalAmount = payDto.TotalAmount,
            NetAmount = payDto.TotalAmount,
            Status = TransactionStatus.Success,
            OrderReference = payDto.OrderReference,
            TransactionDate = DateTime.UtcNow
        };

        _mapperMock.Setup(m => m.Map<PayTransactionCommand>(payDto)).Returns(command);
        _mediatorMock.Setup(m => m.Send(It.Is<PayTransactionCommand>(c =>
            c.BankId == payDto.BankId &&
            c.TotalAmount == payDto.TotalAmount &&
            c.OrderReference == payDto.OrderReference
        ), It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

        // Act
        var result = await _controller.Pay(payDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transaction);

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(payDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<PayTransactionCommand>(c =>
            c.BankId == payDto.BankId &&
            c.TotalAmount == payDto.TotalAmount &&
            c.OrderReference == payDto.OrderReference
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pay_Should_Return_BadRequest_When_Request_Is_Null()
    {
        // Arrange
        PayTransactionDto payDto = null;

        // Act
        var result = await _controller.Pay(payDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Payment details are required.");

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(It.IsAny<PayTransactionDto>()), Times.Never);
        _mediatorMock.Verify(m => m.Send(It.IsAny<PayTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Pay_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var payDto = new PayTransactionDto
        {
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 100m,
            OrderReference = "ORDER123"
        };

        var command = new PayTransactionCommand
        {
            BankId = payDto.BankId,
            TotalAmount = payDto.TotalAmount,
            OrderReference = payDto.OrderReference
        };

        _mapperMock.Setup(m => m.Map<PayTransactionCommand>(payDto)).Returns(command);
        _mediatorMock.Setup(m => m.Send(It.Is<PayTransactionCommand>(c =>
                c.BankId == payDto.BankId &&
                c.TotalAmount == payDto.TotalAmount &&
                c.OrderReference == payDto.OrderReference
            ), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Pay(payDto);

        // Assert
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while processing the payment: Database error");

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(payDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<PayTransactionCommand>(c =>
            c.BankId == payDto.BankId &&
            c.TotalAmount == payDto.TotalAmount &&
            c.OrderReference == payDto.OrderReference
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task Cancel_Should_Return_Ok_With_Transaction_When_Transaction_Is_Found()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = new Transaction
        {
            Id = transactionId,
            NetAmount = 0m,
            Status = TransactionStatus.Success,
            TransactionDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _controller.Cancel(transactionId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transaction);

        _mediatorMock.Verify(
            m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_Should_Return_BadRequest_When_TransactionId_Is_Empty()
    {
        // Arrange
        var transactionId = Guid.Empty;

        // Act
        var result = await _controller.Cancel(transactionId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("A valid transaction ID must be provided.");

        _mediatorMock.Verify(m => m.Send(It.IsAny<CancelTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancel_Should_Return_NotFound_When_Transaction_Is_Not_Found()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction)null);

        // Act
        var result = await _controller.Cancel(transactionId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().Be($"Transaction with ID {transactionId} not found.");

        _mediatorMock.Verify(
            m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cancellation error"));

        // Act
        var result = await _controller.Cancel(transactionId);

        // Assert
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while canceling the transaction: Cancellation error");

        _mediatorMock.Verify(
            m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Refund Tests

    [Fact]
    public async Task Refund_Should_Return_Ok_With_Transaction_When_Transaction_Is_Found()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = new Transaction
        {
            Id = transactionId,
            NetAmount = 0m,
            Status = TransactionStatus.Success,
            TransactionDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _controller.Refund(transactionId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transaction);

        _mediatorMock.Verify(
            m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refund_Should_Return_BadRequest_When_TransactionId_Is_Empty()
    {
        // Arrange
        var transactionId = Guid.Empty;

        // Act
        var result = await _controller.Refund(transactionId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("A valid transaction ID must be provided.");

        _mediatorMock.Verify(m => m.Send(It.IsAny<RefundTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Refund_Should_Return_NotFound_When_Transaction_Is_Not_Found()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        _mediatorMock.Setup(m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction)null);

        // Act
        var result = await _controller.Refund(transactionId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().Be($"Transaction with ID {transactionId} not found.");

        _mediatorMock.Verify(
            m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refund_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        _mediatorMock.Setup(m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Refund error"));

        // Act
        var result = await _controller.Refund(transactionId);

        // Assert
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while processing the refund: Refund error");

        _mediatorMock.Verify(
            m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_Should_Return_Ok_With_Transactions_When_Search_Is_Successful()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status
        };

        var transactions = new List<TransactionReportDto>
        {
            new TransactionReportDto
            {
                TransactionId = Guid.NewGuid(),
                TotalAmount = 100m,
                NetAmount = 100m,
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                OrderReference = "dummy",
                BankId = Guid.NewGuid().ToString(),
                TransactionDetails = new List<TransactionDetailDto>(),
            },
        };

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(searchDto)).Returns(query);
        _mediatorMock.Setup(m => m.Send(It.Is<SearchPaymentQuery>(q =>
            q.StartDate == searchDto.StartDate &&
            q.EndDate == searchDto.EndDate &&
            q.Status == searchDto.Status
        ), It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transactions);

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<SearchPaymentQuery>(q =>
            q.StartDate == searchDto.StartDate &&
            q.EndDate == searchDto.EndDate &&
            q.Status == searchDto.Status
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_Should_Return_BadRequest_When_SearchDto_Is_Null()
    {
        // Arrange
        SearchPaymentDto searchDto = null;

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Search parameters are required.");

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(It.IsAny<SearchPaymentDto>()), Times.Never);
        _mediatorMock.Verify(m => m.Send(It.IsAny<SearchPaymentQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Search_Should_Return_Empty_List_When_No_Transactions_Found()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status
        };

        var transactions = new List<TransactionReportDto>();

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(searchDto)).Returns(query);
        _mediatorMock.Setup(m => m.Send(It.Is<SearchPaymentQuery>(q =>
            q.StartDate == searchDto.StartDate &&
            q.EndDate == searchDto.EndDate &&
            q.Status == searchDto.Status
        ), It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<SearchPaymentQuery>(q =>
            q.StartDate == searchDto.StartDate &&
            q.EndDate == searchDto.EndDate &&
            q.Status == searchDto.Status
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status
        };

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(searchDto)).Returns(query);
        _mediatorMock.Setup(m => m.Send(It.Is<SearchPaymentQuery>(q =>
                q.StartDate == searchDto.StartDate &&
                q.EndDate == searchDto.EndDate &&
                q.Status == searchDto.Status
            ), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Search error"));

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while searching for transactions: Search error");

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<SearchPaymentQuery>(q =>
            q.StartDate == searchDto.StartDate &&
            q.EndDate == searchDto.EndDate &&
            q.Status == searchDto.Status
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}