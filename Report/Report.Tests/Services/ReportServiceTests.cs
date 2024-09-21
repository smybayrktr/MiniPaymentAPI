using Moq;
using Report.Application.Services;
using Report.Contracts.Queries;
using Report.Domain.Entities;
using FluentAssertions;
using Report.Domain.Enums;
using Report.Infrastructure.Clients;

namespace Report.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IPaymentServiceClient> _paymentServiceClientMock;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        _reportService = new ReportService(_paymentServiceClientMock.Object);
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnTransactions_WhenPaymentServiceClientReturnsData()
    {
        // Arrange
        var query = new GetReportQuery
        {
            BankId = "YapiKredi",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_003",
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow
        };

        var expectedTransactions = new List<Transaction>
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                BankId = "YapiKredi",
                TotalAmount = 1500m,
                NetAmount = 1400m,
                Status = TransactionStatus.Success,
                OrderReference = "ORDER_003",
                TransactionDate = DateTime.UtcNow.AddDays(-5)
            }
        };

        _paymentServiceClientMock.Setup(client => client.SearchTransactionsAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _reportService.GetReportAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(expectedTransactions);

        _paymentServiceClientMock.Verify(client => client.SearchTransactionsAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnEmpty_WhenPaymentServiceClientReturnsNoData()
    {
        // Arrange
        var query = new GetReportQuery
        {
            BankId = "Akbank",
            Status = TransactionStatus.Fail,
            OrderReference = "ORDER_004",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow
        };

        var expectedTransactions = new List<Transaction>();

        _paymentServiceClientMock.Setup(client => client.SearchTransactionsAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _reportService.GetReportAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _paymentServiceClientMock.Verify(client => client.SearchTransactionsAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetReportAsync_ShouldThrowException_WhenPaymentServiceClientThrowsException()
    {
        // Arrange
        var query = new GetReportQuery
        {
            BankId = "Garanti",
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_005",
            StartDate = DateTime.UtcNow.AddDays(-25),
            EndDate = DateTime.UtcNow
        };

        _paymentServiceClientMock.Setup(client => client.SearchTransactionsAsync(query))
            .ThrowsAsync(new Exception("Payment Service Error"));

        // Act
        Func<Task> act = async () => { await _reportService.GetReportAsync(query); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Payment Service Error");

        _paymentServiceClientMock.Verify(client => client.SearchTransactionsAsync(query), Times.Once);
    }
}