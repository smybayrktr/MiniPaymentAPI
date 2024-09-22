using System.Transactions;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Report.API.Controllers;
using Report.Contracts.DTOs;
using Report.Contracts.Queries;

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
                Status = TransactionStatusDto.Success,
                OrderReference = "ORDER123",
                TransactionDate = DateTime.UtcNow
            },
            new TransactionReportDto
            {
                TransactionId = Guid.NewGuid(),
                BankId = Guid.NewGuid().ToString(),
                TotalAmount = 1500m,
                NetAmount = 1500m,
                Status = TransactionStatusDto.Success,
                OrderReference = "ORDER456",
                TransactionDate = DateTime.UtcNow
            }
        };
    }

    private GetReportQuery CreateSampleGetReportQuery(GetReportDto request)
    {
        return new GetReportQuery
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            BankId = request.BankId
        };
    }
    
    [Fact]
    public async Task GetReport_ShouldReturnOk_WithTransactionReports_WhenReportGenerationIsSuccessful()
    {
        // Arrange
        var request = new GetReportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatusDto.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = CreateSampleGetReportQuery(request);

        var reportData = CreateSampleTransactionReports();

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _controller.GetReport(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(reportData);

        _mapperMock.Verify(m => m.Map<GetReportQuery>(request), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetReport_ShouldReturnOk_WithEmptyList_WhenNoTransactionsMatchCriteria()
    {
        // Arrange
        var request = new GetReportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatusDto.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = CreateSampleGetReportQuery(request);

        var reportData = new List<TransactionReportDto>(); // Empty list

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _controller.GetReport(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(reportData);

        _mapperMock.Verify(m => m.Map<GetReportQuery>(request), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetReport_ShouldThrowException_WhenMappingFails()
    {
        // Arrange
        var request = new GetReportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatusDto.Success,
            BankId = Guid.NewGuid().ToString()
        };

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Throws(new AutoMapperMappingException("Mapping failed"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.GetReport(request);

        // Assert
        await act.Should().ThrowAsync<AutoMapperMappingException>()
            .WithMessage("Mapping failed");

        _mapperMock.Verify(m => m.Map<GetReportQuery>(request), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetReportQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task GetReport_ShouldThrowException_WhenMediatorThrowsException()
    {
        // Arrange
        var request = new GetReportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatusDto.Success,
            BankId = Guid.NewGuid().ToString()
        };

        var query = CreateSampleGetReportQuery(request);

        _mapperMock.Setup(m => m.Map<GetReportQuery>(It.IsAny<GetReportDto>()))
            .Returns(query);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act
        Func<Task<IActionResult>> act = async () => await _controller.GetReport(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Mediator error");

        _mapperMock.Verify(m => m.Map<GetReportQuery>(request), Times.Once);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}