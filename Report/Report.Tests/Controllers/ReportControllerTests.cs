using Moq;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Report.API.Controllers;
using Report.Contracts.Queries;
using Report.Contracts.DTOs;
using Report.Domain.Entities;
using Report.Domain.Enums;

namespace Report.Tests.Controllers;

public class ReportControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ReportController _controller;

    public ReportControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new ReportController(_mediatorMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetReport_ShouldReturnOk_WithReportData_WhenRequestIsValid()
    {
        // Arrange
        var requestDto = new GetReportDto
        {
            BankId = "Akbank",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_001",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var query = new GetReportQuery
        {
            BankId = requestDto.BankId,
            Status = requestDto.Status,
            OrderReference = requestDto.OrderReference,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate
        };

        var expectedTransactions = new List<Transaction>
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                BankId = "Akbank",
                TotalAmount = 1000m,
                NetAmount = 900m,
                Status = TransactionStatus.Success,
                OrderReference = "ORDER_001",
                TransactionDate = DateTime.UtcNow.AddDays(-10)
            }
        };

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _controller.GetReport(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedTransactions);

        _mapperMock.Verify(m => m.Map<GetReportQuery>(requestDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReport_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        GetReportDto requestDto = null;

        // Act
        var result = await _controller.GetReport(requestDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Report request parameters are required.");

        _mapperMock.Verify(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()), Times.Never);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetReportQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReport_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var requestDto = new GetReportDto
        {
            BankId = "Garanti",
            Status = TransactionStatus.Fail,
            OrderReference = "ORDER_002",
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow
        };

        var query = new GetReportQuery
        {
            BankId = requestDto.BankId,
            Status = requestDto.Status,
            OrderReference = requestDto.OrderReference,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate
        };

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator Error"));

        // Act
        var result = await _controller.GetReport(requestDto);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while generating the report: Mediator Error");

        _mapperMock.Verify(m => m.Map<GetReportQuery>(requestDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReport_ShouldReturnOk_WithEmptyList_WhenNoTransactionsFound()
    {
        // Arrange
        var requestDto = new GetReportDto
        {
            BankId = "YapiKredi",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_003",
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow
        };

        var query = new GetReportQuery
        {
            BankId = requestDto.BankId,
            Status = requestDto.Status,
            OrderReference = requestDto.OrderReference,
            StartDate = requestDto.StartDate,
            EndDate = requestDto.EndDate
        };

        var expectedTransactions = new List<Transaction>(); // Boş liste

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _controller.GetReport(requestDto);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedTransactions);

        _mapperMock.Verify(m => m.Map<GetReportQuery>(requestDto), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}