using Moq;
using Payment.Application.Exceptions;
using Payment.Application.Interfaces;
using Payment.Application.Services;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;

namespace Payment.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IBankFactory> _bankFactoryMock;
    private readonly Mock<IBankService> _bankServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _bankFactoryMock = new Mock<IBankFactory>();
        _bankServiceMock = new Mock<IBankService>();

        _bankFactoryMock.Setup(f => f.GetBankService(It.IsAny<string>()))
            .Returns(_bankServiceMock.Object);

        _paymentService = new PaymentService(_transactionRepositoryMock.Object, _bankFactoryMock.Object);
    }

    // Helper method to create a sample transaction
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

    // Helper method to create a sample PayTransactionCommand
    private PayTransactionCommand CreatePayTransactionCommand(string bankId)
    {
        return new PayTransactionCommand
        {
            BankId = bankId,
            TotalAmount = 1000m,
            OrderReference = "ORDER123"
        };
    }

    [Fact]
    public async Task PayAsync_ShouldProcessPaymentSuccessfully()
    {
        // Arrange
        var bankId = Guid.NewGuid().ToString();
        var command = CreatePayTransactionCommand(bankId);
        var expectedTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            BankId = bankId,
            TotalAmount = command.TotalAmount,
            NetAmount = command.TotalAmount,
            Status = TransactionStatus.Success,
            OrderReference = command.OrderReference,
            TransactionDate = DateTime.UtcNow
        };

        _bankServiceMock.Setup(s => s.PayAsync(It.IsAny<PayTransactionCommand>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _paymentService.PayAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTransaction.Id, result.Id);
        Assert.Equal(TransactionStatus.Success, result.Status);
        _bankFactoryMock.Verify(f => f.GetBankService(bankId), Times.Once);
        _bankServiceMock.Verify(s => s.PayAsync(command), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelPaymentSuccessfully()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transaction = CreateSampleTransaction(transactionId, bankId, DateTime.UtcNow);

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        _transactionRepositoryMock.Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);

        _bankServiceMock.Setup(s => s.CancelAsync(transaction))
            .ReturnsAsync(transaction);

        // Act
        var result = await _paymentService.CancelAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
        Assert.Equal(TransactionStatus.Success, result.Status);
        _transactionRepositoryMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        _bankFactoryMock.Verify(f => f.GetBankService(bankId), Times.Once);
        _bankServiceMock.Verify(s => s.CancelAsync(transaction), Times.Once);
    }

    [Fact]
    public async Task RefundAsync_ShouldRefundPaymentSuccessfully()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow.AddDays(-2); // Ensure it's after one day
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        _transactionRepositoryMock.Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);

        _bankServiceMock.Setup(s => s.RefundAsync(transaction))
            .ReturnsAsync(transaction);

        // Act
        var result = await _paymentService.RefundAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
        Assert.Equal(TransactionStatus.Success, result.Status);
        _transactionRepositoryMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        _bankFactoryMock.Verify(f => f.GetBankService(bankId), Times.Once);
        _bankServiceMock.Verify(s => s.RefundAsync(transaction), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowTransactionNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        _transactionRepositoryMock.Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync((Transaction)null);

        // Act & Assert
        await Assert.ThrowsAsync<TransactionNotFoundException>(() => _paymentService.CancelAsync(command));
        _transactionRepositoryMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        _bankFactoryMock.Verify(f => f.GetBankService(It.IsAny<string>()), Times.Never);
        _bankServiceMock.Verify(s => s.CancelAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task RefundAsync_ShouldThrowTransactionNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var command = new RefundTransactionCommand
        {
            TransactionId = transactionId
        };

        _transactionRepositoryMock.Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync((Transaction)null);

        // Act & Assert
        await Assert.ThrowsAsync<TransactionNotFoundException>(() => _paymentService.RefundAsync(command));
        _transactionRepositoryMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        _bankFactoryMock.Verify(f => f.GetBankService(It.IsAny<string>()), Times.Never);
        _bankServiceMock.Verify(s => s.RefundAsync(It.IsAny<Transaction>()), Times.Never);
    }
    
    [Fact]
    public async Task CancelAsync_ShouldThrowBusinessLogicException_WhenCancellationDateIsDifferent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var bankId = Guid.NewGuid().ToString();
        var transactionDate = DateTime.UtcNow.AddDays(-1); // Different day
        var transaction = CreateSampleTransaction(transactionId, bankId, transactionDate);

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        _transactionRepositoryMock.Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);

        _bankServiceMock.Setup(s => s.CancelAsync(transaction))
            .ThrowsAsync(new BusinessLogicException("Cancel operation is only allowed on the same day"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessLogicException>(() => _paymentService.CancelAsync(command));
        Assert.Equal("Cancel operation is only allowed on the same day", exception.Message);
        _transactionRepositoryMock.Verify(r => r.GetByIdAsync(transactionId), Times.Once);
        _bankFactoryMock.Verify(f => f.GetBankService(bankId), Times.Once);
        _bankServiceMock.Verify(s => s.CancelAsync(transaction), Times.Once);
    }

}