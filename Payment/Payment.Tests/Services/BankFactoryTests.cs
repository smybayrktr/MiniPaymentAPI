using Moq;
using Payment.Application.Services;
using FluentAssertions;
using Payment.Infrastructure.Repositories;

namespace Payment.Tests.Services;

public class BankFactoryTests
{
    private readonly Mock<AkbankService> _akbankServiceMock;
    private readonly Mock<GarantiService> _garantiServiceMock;
    private readonly Mock<YapiKrediService> _yapiKrediServiceMock;
    private readonly BankFactory _bankFactory;

    public BankFactoryTests()
    {
        _akbankServiceMock = new Mock<AkbankService>(MockBehavior.Strict, new Mock<ITransactionRepository>().Object,
            new Mock<ITransactionDetailRepository>().Object);
        _garantiServiceMock = new Mock<GarantiService>(MockBehavior.Strict, new Mock<ITransactionRepository>().Object,
            new Mock<ITransactionDetailRepository>().Object);
        _yapiKrediServiceMock = new Mock<YapiKrediService>(MockBehavior.Strict,
            new Mock<ITransactionRepository>().Object, new Mock<ITransactionDetailRepository>().Object);

        _bankFactory = new BankFactory(
            _akbankServiceMock.Object,
            _garantiServiceMock.Object,
            _yapiKrediServiceMock.Object
        );
    }

    [Theory]
    [InlineData("Akbank")]
    [InlineData("Garanti")]
    [InlineData("YapiKredi")]
    public void GetBankService_ShouldReturnCorrectService_ForValidBankId(string bankId)
    {
        // Arrange & Act
        var result = _bankFactory.GetBankService(bankId);

        // Assert
        switch (bankId)
        {
            case "Akbank":
                result.Should().Be(_akbankServiceMock.Object);
                break;
            case "Garanti":
                result.Should().Be(_garantiServiceMock.Object);
                break;
            case "YapiKredi":
                result.Should().Be(_yapiKrediServiceMock.Object);
                break;
            default:
                throw new ArgumentException("Invalid Bank Id");
        }
    }

    [Fact]
    public void GetBankService_ShouldThrowArgumentException_ForInvalidBankId()
    {
        // Arrange
        var invalidBankId = "InvalidBank";

        // Act
        Action act = () => _bankFactory.GetBankService(invalidBankId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid Bank Id");
    }
}