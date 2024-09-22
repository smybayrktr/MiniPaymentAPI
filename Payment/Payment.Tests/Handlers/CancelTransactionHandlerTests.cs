using FluentAssertions;
using Moq;
using Payment.Application.Handlers;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Tests.Handlers;

public class CancelTransactionHandlerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ITimeZoneService> _timeZoneServiceMock;
    private readonly CancelTransactionHandler _handler;

    public CancelTransactionHandlerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _timeZoneServiceMock = new Mock<ITimeZoneService>();
        _handler = new CancelTransactionHandler(_paymentServiceMock.Object, _timeZoneServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_CancelAsync_And_Return_Transaction()
    {
        // Arrange
        var command = new CancelTransactionCommand
        {
            TransactionId = Guid.NewGuid()
        };

        var expectedTransaction = new Transaction
        {
            Id = command.TransactionId,
            NetAmount = 0m,
            Status = TransactionStatus.Success,
            TransactionDate = DateTime.UtcNow
        };

        _paymentServiceMock
            .Setup(service => service.CancelAsync(command))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentServiceMock.Verify(service => service.CancelAsync(command), Times.Once);
        result.Should().BeEquivalentTo(expectedTransaction);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_CancelAsync_Fails()
    {
        // Arrange
        var command = new CancelTransactionCommand
        {
            TransactionId = Guid.NewGuid()
        };

        _paymentServiceMock
            .Setup(service => service.CancelAsync(command))
            .ThrowsAsync(new InvalidOperationException("Cancel failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cancel failed");

        _paymentServiceMock.Verify(service => service.CancelAsync(command), Times.Once);
    }
}
