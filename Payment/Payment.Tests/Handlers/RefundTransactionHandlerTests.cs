using FluentAssertions;
using Moq;
using Payment.Application.Handlers;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Tests.Handlers;

public class RefundTransactionHandlerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ITimeZoneService> _timeZoneServiceMock;
    private readonly RefundTransactionHandler _handler;

    public RefundTransactionHandlerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _timeZoneServiceMock = new Mock<ITimeZoneService>();
        _handler = new RefundTransactionHandler(_paymentServiceMock.Object, _timeZoneServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_RefundAsync_And_Return_Transaction()
    {
        // Arrange
        var command = new RefundTransactionCommand
        {
            TransactionId = Guid.NewGuid(),
        };

        var expectedTransaction = new Transaction
        {
            Id = command.TransactionId,
            NetAmount = 0m,
            Status = TransactionStatus.Success,
            TransactionDate = DateTime.UtcNow
        };

        _paymentServiceMock
            .Setup(service => service.RefundAsync(command))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentServiceMock.Verify(service => service.RefundAsync(command), Times.Once);
        result.Should().BeEquivalentTo(expectedTransaction);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_RefundAsync_Fails()
    {
        // Arrange
        var command = new RefundTransactionCommand
        {
            TransactionId = Guid.NewGuid(),
        };

        _paymentServiceMock
            .Setup(service => service.RefundAsync(command))
            .ThrowsAsync(new InvalidOperationException("Refund failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refund failed");

        _paymentServiceMock.Verify(service => service.RefundAsync(command), Times.Once);
    }
}
