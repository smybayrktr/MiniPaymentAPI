using Moq;
using Payment.Application.Services;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;
using FluentAssertions;
using Payment.Application.Exceptions;
using Payment.Application.Interfaces;

namespace Payment.Tests.Services;

public class GarantiServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ITransactionDetailRepository> _transactionDetailRepositoryMock;
    private readonly GarantiService _garantiService;

    public GarantiServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionDetailRepositoryMock = new Mock<ITransactionDetailRepository>();
        _garantiService = new GarantiService(_transactionRepositoryMock.Object, _transactionDetailRepositoryMock.Object);
    }

    private PayTransactionCommand CreatePayTransactionCommand(string bankId)
    {
        return new PayTransactionCommand
        {
            BankId = bankId,
            TotalAmount = 1000m,
            OrderReference = "ORDER123"
        };
    }

    private Transaction CreateSampleTransaction(Guid transactionId, string bankId, DateTime transactionDate)
    {
        return new Transaction
        {
            Id = transactionId,
            BankId = bankId,
            TotalAmount = 1000m,
            NetAmount = 1000m,
            Status = TransactionStatus.Success,
            OrderReference = "ORDER123",
            TransactionDate = transactionDate
        };
    }

    [Fact]
    public async Task PayAsync_ShouldCreateTransactionAndTransactionDetail()
    {
        // Arrange
        var bankId = Guid.NewGuid().ToString();
        var command = CreatePayTransactionCommand(bankId);

        // Act
        var result = await _garantiService.PayAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bankId, result.BankId);
        Assert.Equal(command.TotalAmount, result.TotalAmount);
        Assert.Equal(TransactionStatus.Success, result.Status);
        Assert.Equal(command.OrderReference, result.OrderReference);
        _transactionRepositoryMock.Verify(r => r.AddAsync(It.Is<Transaction>(t => t.Id == result.Id)), Times.Once);
        _transactionDetailRepositoryMock.Verify(
            d => d.AddAsync(It.Is<TransactionDetail>(td =>
                td.TransactionId == result.Id && td.TransactionType == TransactionType.Sale)), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldUpdateTransactionAndAddTransactionDetail()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow;
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        // Act
        var result = await _garantiService.CancelAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransactionStatus.Success, result.Status);
        Assert.Equal(0m, result.NetAmount);
        _transactionRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Transaction>(t => t.Id == transactionId && t.NetAmount == 0m)), Times.Once);
        _transactionDetailRepositoryMock.Verify(
            d => d.AddAsync(It.Is<TransactionDetail>(td =>
                td.TransactionId == transactionId && td.TransactionType == TransactionType.Cancel)), Times.Once);
    }

    [Fact]
    public async Task RefundAsync_ShouldUpdateTransactionAndAddTransactionDetail()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow.AddDays(-2);
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        // Act
        var result = await _garantiService.RefundAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransactionStatus.Success, result.Status);
        Assert.Equal(0m, result.NetAmount);
        _transactionRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Transaction>(t => t.Id == transactionId && t.NetAmount == 0m)), Times.Once);
        _transactionDetailRepositoryMock.Verify(
            d => d.AddAsync(It.Is<TransactionDetail>(td =>
                td.TransactionId == transactionId && td.TransactionType == TransactionType.Refund)), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowBusinessLogicException_WhenTransactionDateIsDifferent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow.AddDays(-1);
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessLogicException>(() => _garantiService.CancelAsync(transaction));
        Assert.Equal("Cancel operation is only allowed on the same day", exception.Message);
        _transactionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(d => d.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }

    [Fact]
    public async Task RefundAsync_ShouldThrowBusinessLogicException_WhenRefundIsWithinOneDay()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow.AddHours(-12);
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessLogicException>(() => _garantiService.RefundAsync(transaction));
        Assert.Equal("Refund operation is allowed only after one day", exception.Message);
        _transactionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(d => d.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }
}