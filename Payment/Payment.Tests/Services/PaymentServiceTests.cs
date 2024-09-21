using Moq;
using Payment.Application.Services;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Infrastructure.Repositories;
using FluentAssertions;
using Payment.Domain.Enums;

namespace Payment.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IBankFactory> _bankFactoryMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _bankFactoryMock = new Mock<IBankFactory>();
            _paymentService = new PaymentService(_transactionRepositoryMock.Object, _bankFactoryMock.Object);
        }

        #region PayAsync Tests

        [Fact]
        public async Task PayAsync_ShouldReturnTransaction_WhenBankServiceSucceeds()
        {
            // Arrange
            var command = new PayTransactionCommand
            {
                BankId = "Akbank",
                TotalAmount = 100.00m,
                OrderReference = "ORDER123"
            };

            var expectedTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                BankId = command.BankId,
                TotalAmount = command.TotalAmount,
                OrderReference = command.OrderReference,
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow
            };

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.PayAsync(command)).ReturnsAsync(expectedTransaction);

            _bankFactoryMock.Setup(bf => bf.GetBankService(command.BankId)).Returns(bankServiceMock.Object);

            // Act
            var result = await _paymentService.PayAsync(command);

            // Assert
            result.Should().BeEquivalentTo(expectedTransaction);
            _bankFactoryMock.Verify(bf => bf.GetBankService(command.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.PayAsync(command), Times.Once);
        }

        [Fact]
        public async Task PayAsync_ShouldThrowException_WhenBankServiceThrows()
        {
            // Arrange
            var command = new PayTransactionCommand
            {
                BankId = "Akbank",
                TotalAmount = 100.00m,
                OrderReference = "ORDER123"
            };

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.PayAsync(command)).ThrowsAsync(new Exception("Payment failed"));

            _bankFactoryMock.Setup(bf => bf.GetBankService(command.BankId)).Returns(bankServiceMock.Object);

            // Act
            Func<Task> act = async () => { await _paymentService.PayAsync(command); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Payment failed");
            _bankFactoryMock.Verify(bf => bf.GetBankService(command.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.PayAsync(command), Times.Once);
        }

        #endregion

        #region CancelAsync Tests

        [Fact]
        public async Task CancelAsync_ShouldReturnTransaction_WhenCancellationSucceeds()
        {
            // Arrange
            var command = new CancelTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            var existingTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = "Garanti",
                TotalAmount = 150.00m,
                NetAmount = 150.00m,
                OrderReference = "ORDER456",
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow.AddHours(-1)
            };

            var expectedTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = existingTransaction.BankId,
                TotalAmount = existingTransaction.TotalAmount,
                NetAmount = 0.00m, // NetAmount -= TotalAmount
                OrderReference = existingTransaction.OrderReference,
                Status = TransactionStatus.Success,
                TransactionDate = existingTransaction.TransactionDate
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync(existingTransaction);

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.CancelAsync(command.TransactionId))
                .ReturnsAsync(expectedTransaction);

            _bankFactoryMock.Setup(bf => bf.GetBankService(existingTransaction.BankId))
                .Returns(bankServiceMock.Object);

            // Act
            var result = await _paymentService.CancelAsync(command);

            // Assert
            result.Should().BeEquivalentTo(expectedTransaction);
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(existingTransaction.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.CancelAsync(command.TransactionId), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_ShouldThrowException_WhenTransactionNotFound()
        {
            // Arrange
            var command = new CancelTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync((Transaction)null);

            // Act
            Func<Task> act = async () => { await _paymentService.CancelAsync(command); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Transaction not found");
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_ShouldThrowException_WhenBankServiceThrows()
        {
            // Arrange
            var command = new CancelTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            var existingTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = "Garanti",
                TotalAmount = 150.00m,
                NetAmount = 150.00m,
                OrderReference = "ORDER456",
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow.AddHours(-1)
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync(existingTransaction);

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.CancelAsync(command.TransactionId))
                .ThrowsAsync(new Exception("Cancellation failed"));

            _bankFactoryMock.Setup(bf => bf.GetBankService(existingTransaction.BankId))
                .Returns(bankServiceMock.Object);

            // Act
            Func<Task> act = async () => { await _paymentService.CancelAsync(command); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Cancellation failed");
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(existingTransaction.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.CancelAsync(command.TransactionId), Times.Once);
        }

        #endregion

        #region RefundAsync Tests

        [Fact]
        public async Task RefundAsync_ShouldReturnTransaction_WhenRefundSucceeds()
        {
            // Arrange
            var command = new RefundTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            var existingTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = "YapiKredi",
                TotalAmount = 200.00m,
                NetAmount = 200.00m,
                OrderReference = "ORDER789",
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow.AddDays(-2)
            };

            var expectedTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = existingTransaction.BankId,
                TotalAmount = existingTransaction.TotalAmount,
                NetAmount = 0.00m, // NetAmount -= TotalAmount
                OrderReference = existingTransaction.OrderReference,
                Status = TransactionStatus.Success,
                TransactionDate = existingTransaction.TransactionDate
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync(existingTransaction);

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.RefundAsync(command.TransactionId))
                .ReturnsAsync(expectedTransaction);

            _bankFactoryMock.Setup(bf => bf.GetBankService(existingTransaction.BankId))
                .Returns(bankServiceMock.Object);

            // Act
            var result = await _paymentService.RefundAsync(command);

            // Assert
            result.Should().BeEquivalentTo(expectedTransaction);
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(existingTransaction.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.RefundAsync(command.TransactionId), Times.Once);
        }

        [Fact]
        public async Task RefundAsync_ShouldThrowException_WhenTransactionNotFound()
        {
            // Arrange
            var command = new RefundTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync((Transaction)null);

            // Act
            Func<Task> act = async () => { await _paymentService.RefundAsync(command); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Transaction not found");
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefundAsync_ShouldThrowException_WhenBankServiceThrows()
        {
            // Arrange
            var command = new RefundTransactionCommand
            {
                TransactionId = Guid.NewGuid()
            };

            var existingTransaction = new Transaction
            {
                Id = command.TransactionId,
                BankId = "YapiKredi",
                TotalAmount = 200.00m,
                NetAmount = 200.00m,
                OrderReference = "ORDER789",
                Status = TransactionStatus.Success,
                TransactionDate = DateTime.UtcNow.AddDays(-2)
            };

            _transactionRepositoryMock.Setup(tr => tr.GetByIdAsync(command.TransactionId))
                .ReturnsAsync(existingTransaction);

            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(bs => bs.RefundAsync(command.TransactionId))
                .ThrowsAsync(new Exception("Refund failed"));

            _bankFactoryMock.Setup(bf => bf.GetBankService(existingTransaction.BankId))
                .Returns(bankServiceMock.Object);

            // Act
            Func<Task> act = async () => { await _paymentService.RefundAsync(command); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Refund failed");
            _transactionRepositoryMock.Verify(tr => tr.GetByIdAsync(command.TransactionId), Times.Once);
            _bankFactoryMock.Verify(bf => bf.GetBankService(existingTransaction.BankId), Times.Once);
            bankServiceMock.Verify(bs => bs.RefundAsync(command.TransactionId), Times.Once);
        }

        #endregion
    }
}
