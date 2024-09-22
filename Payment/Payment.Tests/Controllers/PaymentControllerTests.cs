using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Payment.API.Controllers;
using Payment.Application.Exceptions;
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

    private Transaction CreateSampleTransaction()
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 1000m,
            NetAmount = 1000m,
            Status = Domain.Enums.TransactionStatus.Success,
            OrderReference = "ORDER123",
            TransactionDate = DateTime.UtcNow
        };
    }

    private IEnumerable<TransactionReportDto> CreateSampleTransactionReports()
    {
        return new List<TransactionReportDto>
        {
            new TransactionReportDto
            {
                TransactionId = Guid.NewGuid(),
                BankId = Guid.NewGuid().ToString(),
                TotalAmount = 500m,
                NetAmount = 500m,
                Status = TransactionStatus.Success,
                OrderReference = "ORDER123",
                TransactionDate = DateTime.UtcNow
            },
            new TransactionReportDto
            {
                TransactionId = Guid.NewGuid(),
                BankId = Guid.NewGuid().ToString(),
                TotalAmount = 1500m,
                NetAmount = 1500m,
                Status = TransactionStatus.Success,
                OrderReference = "ORDER456",
                TransactionDate = DateTime.UtcNow
            }
        };
    }

    #region Pay Tests

    [Fact]
    public async Task Pay_ShouldReturnOk_WithTransaction_WhenPaymentIsSuccessful()
    {
        // Arrange
        var payDto = new PayTransactionDto
        {
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 1000m,
            OrderReference = "ORDER123"
        };

        var command = new PayTransactionCommand
        {
            BankId = payDto.BankId,
            TotalAmount = payDto.TotalAmount,
            OrderReference = payDto.OrderReference
        };

        var transaction = CreateSampleTransaction();

        _mapperMock.Setup(m => m.Map<PayTransactionCommand>(It.IsAny<PayTransactionDto>()))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<PayTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _controller.Pay(payDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transaction);

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(payDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pay_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var payDto = new PayTransactionDto
        {
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 1000m,
            OrderReference = "ORDER123"
        };

        _mapperMock.Setup(m => m.Map<PayTransactionCommand>(It.IsAny<PayTransactionDto>()))
            .Throws(new AutoMapperMappingException("Mapping failed"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Pay(payDto);

        // Assert
        await act.Should().ThrowAsync<AutoMapperMappingException>();

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(payDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<PayTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Pay_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var payDto = new PayTransactionDto
        {
            BankId = Guid.NewGuid().ToString(),
            TotalAmount = 1000m,
            OrderReference = "ORDER123"
        };

        var command = new PayTransactionCommand
        {
            BankId = payDto.BankId,
            TotalAmount = payDto.TotalAmount,
            OrderReference = payDto.OrderReference
        };

        _mapperMock.Setup(m => m.Map<PayTransactionCommand>(It.IsAny<PayTransactionDto>()))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<PayTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Pay(payDto);

        // Assert
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.WithMessage("Mediator error");

        _mapperMock.Verify(m => m.Map<PayTransactionCommand>(payDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task Cancel_ShouldReturnOk_WithUpdatedTransaction_WhenCancellationIsSuccessful()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = CreateSampleTransaction();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelTransactionCommand>(), It.IsAny<CancellationToken>()))
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
    public async Task Cancel_ShouldReturnNotFound_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TransactionNotFoundException("Transaction not found"));

        // Act
        try
        {
            await _controller.Cancel(transactionId);
        }
        catch (Exception e)
        {
            e.Should().BeOfType<TransactionNotFoundException>();
            _mediatorMock.Verify(
                m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                    It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task Cancel_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Cancel(transactionId);

        // Assert
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.WithMessage("Mediator error");

        _mediatorMock.Verify(
            m => m.Send(It.Is<CancelTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Refund Tests

    [Fact]
    public async Task Refund_ShouldReturnOk_WithUpdatedTransaction_WhenRefundIsSuccessful()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        var transaction = CreateSampleTransaction();

        _mapperMock.Setup(m => m.Map<RefundTransactionCommand>(It.IsAny<Guid>()))
            .Returns(command);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefundTransactionCommand>(), It.IsAny<CancellationToken>()))
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
    public async Task Refund_ShouldReturnNotFound_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefundTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TransactionNotFoundException("Transaction not found"));

        try
        {
            await _controller.Refund(transactionId);
        }
        catch (Exception e)
        {
            e.Should().BeOfType<TransactionNotFoundException>();
            _mediatorMock.Verify(
                m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                    It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task Refund_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefundTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Refund(transactionId);

        // Assert
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.WithMessage("Mediator error");

        _mediatorMock.Verify(
            m => m.Send(It.Is<RefundTransactionCommand>(c => c.TransactionId == transactionId),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ShouldReturnOk_WithTransactionReports_WhenSearchIsSuccessful()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status,
            BankId = searchDto.BankId
        };

        var transactionReports = CreateSampleTransactionReports();

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(It.IsAny<SearchPaymentDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchPaymentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionReports);

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transactionReports);

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_ShouldReturnOk_WithEmptyList_WhenNoTransactionsFound()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status,
            BankId = searchDto.BankId
        };

        var transactionReports = new List<TransactionReportDto>();

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(It.IsAny<SearchPaymentDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchPaymentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionReports);

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(transactionReports);

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
            BankId = Guid.NewGuid().ToString()
        };

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(It.IsAny<SearchPaymentDto>()))
            .Throws(new AutoMapperMappingException("Mapping failed"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Search(searchDto);

        // Assert
        await act.Should().ThrowAsync<AutoMapperMappingException>();

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<SearchPaymentQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Search_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var searchDto = new SearchPaymentDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = new SearchPaymentQuery
        {
            StartDate = searchDto.StartDate,
            EndDate = searchDto.EndDate,
            Status = searchDto.Status,
            BankId = searchDto.BankId
        };

        _mapperMock.Setup(m => m.Map<SearchPaymentQuery>(It.IsAny<SearchPaymentDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchPaymentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.Search(searchDto);

        // Assert
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.WithMessage("Mediator error");

        _mapperMock.Verify(m => m.Map<SearchPaymentQuery>(searchDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}