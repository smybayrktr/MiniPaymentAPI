using FluentAssertions;
using Moq;
using Payment.Application.Handlers;
using Payment.Application.Interfaces;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;

namespace Payment.Tests.Handlers;

public class SearchPaymentHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly Mock<ITimeZoneService> _timeZoneServiceMock;
    private readonly SearchPaymentHandler _handler;

    public SearchPaymentHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _timeZoneServiceMock = new Mock<ITimeZoneService>();
        _handler = new SearchPaymentHandler(_searchServiceMock.Object, _timeZoneServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_SearchAsync_And_Return_Transactions()
    {
        // Arrange
        var query = new SearchPaymentQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success
        };

        var expectedTransactions = new List<TransactionReportDto>
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

        _searchServiceMock
            .Setup(repo => repo.SearchAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _searchServiceMock.Verify(repo => repo.SearchAsync(query), Times.Once);
        result.Should().BeEquivalentTo(expectedTransactions);
    }
    
    [Fact]
    public async Task Handle_Should_Call_SearchAsync_And_Return_Transactions_NotUTC()
    {
        // Arrange
        var query = new SearchPaymentQuery
        {
            StartDate = DateTime.Now.AddDays(-30),
            EndDate = DateTime.Now,
            Status = TransactionStatus.Success
        };

        var expectedTransactions = new List<TransactionReportDto>
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

        _searchServiceMock
            .Setup(repo => repo.SearchAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _searchServiceMock.Verify(repo => repo.SearchAsync(query), Times.Once);
        result.Should().BeEquivalentTo(expectedTransactions);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Transactions_Found()
    {
        // Arrange
        var query = new SearchPaymentQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
        };

        var expectedTransactions = new List<TransactionReportDto>();

        _searchServiceMock
            .Setup(repo => repo.SearchAsync(query))
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _searchServiceMock.Verify(repo => repo.SearchAsync(query), Times.Once);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_SearchAsync_Fails()
    {
        // Arrange
        var query = new SearchPaymentQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Status = TransactionStatus.Success,
        };

        _searchServiceMock
            .Setup(repo => repo.SearchAsync(query))
            .ThrowsAsync(new InvalidOperationException("Search failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Search failed");

        _searchServiceMock.Verify(repo => repo.SearchAsync(query), Times.Once);
    }
}