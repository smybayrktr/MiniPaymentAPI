using Moq;
using Payment.Application.Handlers;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using FluentAssertions;
using Payment.Application.Interfaces;

namespace Payment.Tests.Handlers;

public class PayTransactionHandlerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly PayTransactionHandler _handler;

    public PayTransactionHandlerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _handler = new PayTransactionHandler(_paymentServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransaction_WhenPaymentServiceSucceeds()
    {
        // Arrange
        var command = new PayTransactionCommand
        {
            BankId = "BANK123",
            TotalAmount = 100.50m,
            OrderReference = "ORDER456"
        };

        var expectedTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            BankId = command.BankId,
            TotalAmount = command.TotalAmount,
            OrderReference = command.OrderReference
        };

        _paymentServiceMock
            .Setup(service => service.PayAsync(It.IsAny<PayTransactionCommand>()))
            .ReturnsAsync(expectedTransaction);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedTransaction);
        _paymentServiceMock.Verify(service => service.PayAsync(command), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPaymentServiceFails()
    {
        // Arrange
        var command = new PayTransactionCommand
        {
            BankId = "BANK123",
            TotalAmount = 100.50m,
            OrderReference = "ORDER456"
        };

        _paymentServiceMock
            .Setup(service => service.PayAsync(It.IsAny<PayTransactionCommand>()))
            .ThrowsAsync(new Exception("Payment processing failed"));

        // Act
        Func<Task> act = async () => { await _handler.Handle(command, CancellationToken.None); };

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Payment processing failed");
        _paymentServiceMock.Verify(service => service.PayAsync(command), Times.Once);
    }
}
