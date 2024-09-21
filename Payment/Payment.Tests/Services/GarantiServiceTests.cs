using Moq;
using Payment.Application.Services;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;
using FluentAssertions;

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

    [Fact]
    public async Task PayAsync_ShouldCreateTransactionAndTransactionDetail_WhenCommandIsValid()
    {
        // Arrange
        var command = new PayTransactionCommand
        {
            BankId = "Garanti",
            TotalAmount = 300.00m,
            OrderReference = "ORDER_GTN_001"
        };

        // Act
        var result = await _garantiService.PayAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.BankId.Should().Be(command.BankId);
        result.TotalAmount.Should().Be(command.TotalAmount);
        result.NetAmount.Should().Be(command.TotalAmount);
        result.Status.Should().Be(TransactionStatus.Success);
        result.OrderReference.Should().Be(command.OrderReference);
        result.TransactionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldUpdateTransactionAndCreateTransactionDetail_WhenTransactionExistsAndSameDay()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var existingTransaction = new Transaction
        {
            Id = transactionId,
            BankId = "Garanti",
            TotalAmount = 300.00m,
            NetAmount = 300.00m,
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_GTN_001",
            TransactionDate = DateTime.UtcNow
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(existingTransaction);

        // Act
        var result = await _garantiService.CancelAsync(transactionId);

        // Assert
        result.Should().NotBeNull();
        result.NetAmount.Should().Be(0.00m);
        result.Status.Should().Be(TransactionStatus.Success);

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(existingTransaction), Times.Once);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync((Transaction)null);

        // Act
        Func<Task> act = async () => { await _garantiService.CancelAsync(transactionId); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Transaction not found");

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowException_WhenTransactionIsNotSameDay()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var existingTransaction = new Transaction
        {
            Id = transactionId,
            BankId = "Garanti",
            TotalAmount = 300.00m,
            NetAmount = 300.00m,
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_GTN_001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(existingTransaction);

        // Act
        Func<Task> act = async () => { await _garantiService.CancelAsync(transactionId); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Cancel operation is only allowed on the same day");

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }

    [Fact]
    public async Task RefundAsync_ShouldUpdateTransactionAndCreateTransactionDetail_WhenTransactionExistsAndAfterOneDay()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var existingTransaction = new Transaction
        {
            Id = transactionId,
            BankId = "Garanti",
            TotalAmount = 300.00m,
            NetAmount = 300.00m,
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_GTN_001",
            TransactionDate = DateTime.UtcNow.AddDays(-2)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(existingTransaction);

        // Act
        var result = await _garantiService.RefundAsync(transactionId);

        // Assert
        result.Should().NotBeNull();
        result.NetAmount.Should().Be(0.00m);
        result.Status.Should().Be(TransactionStatus.Success);

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(existingTransaction), Times.Once);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Once);
    }

    [Fact]
    public async Task RefundAsync_ShouldThrowException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync((Transaction)null);

        // Act
        Func<Task> act = async () => { await _garantiService.RefundAsync(transactionId); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Transaction not found");

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }

    [Fact]
    public async Task RefundAsync_ShouldThrowException_WhenRefundNotAllowed()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var existingTransaction = new Transaction
        {
            Id = transactionId,
            BankId = "Garanti",
            TotalAmount = 300.00m,
            NetAmount = 300.00m,
            Status = TransactionStatus.Success,
            OrderReference = "ORDER_GTN_001",
            TransactionDate = DateTime.UtcNow.AddHours(-12)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(existingTransaction);

        // Act
        Func<Task> act = async () => { await _garantiService.RefundAsync(transactionId); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Refund operation is allowed only after one day");

        _transactionRepositoryMock.Verify(repo => repo.GetByIdAsync(transactionId), Times.Once);
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionDetailRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TransactionDetail>()), Times.Never);
    }
}
