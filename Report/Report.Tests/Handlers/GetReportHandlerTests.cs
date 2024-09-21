using Moq;
using FluentAssertions;
using Report.Application.Handlers;
using Report.Application.Interfaces;
using Report.Contracts.Queries;
using Report.Domain.Entities;
using Report.Domain.Enums;

namespace Report.Tests.Handlers;

public class GetReportHandlerTests
{
    private readonly Mock<IReportService> _reportServiceMock;
    private readonly GetReportHandler _handler;

    public GetReportHandlerTests()
    {
        _reportServiceMock = new Mock<IReportService>();
        _handler = new GetReportHandler(_reportServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransactions_WhenReportServiceReturnsData()
    {
        // Arrange
        var query = new GetReportQuery
        {
            BankId = "Akbank",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_001",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
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

        _reportServiceMock.Setup(service => service.GetReportAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedTransactions);

        _reportServiceMock.Verify(service => service.GetReportAsync(query), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenReportServiceThrowsException()
    {
        // Arrange
        var query = new GetReportQuery
        {
            BankId = "Garanti",
            Status = TransactionStatus.Fail,
            OrderReference = "ORDER_002",
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow
        };

        _reportServiceMock.Setup(service => service.GetReportAsync(query))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        Func<Task> act = async () => { await _handler.Handle(query, CancellationToken.None); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Service error");

        _reportServiceMock.Verify(service => service.GetReportAsync(query), Times.Once);
    }
}