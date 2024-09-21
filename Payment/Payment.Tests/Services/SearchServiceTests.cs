using FluentAssertions;
using Moq;
using Payment.Application.Services;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;

namespace Payment.Tests.Services
{
    public class SearchServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ITransactionDetailRepository> _transactionDetailRepositoryMock;
        private readonly SearchService _searchService;

        public SearchServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _transactionDetailRepositoryMock = new Mock<ITransactionDetailRepository>();
            _searchService =
                new SearchService(_transactionRepositoryMock.Object, _transactionDetailRepositoryMock.Object);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnAllFilteredTransactionsWithDetails()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    BankId = "Akbank",
                    TotalAmount = 1000.00m,
                    NetAmount = 1000.00m,
                    Status = TransactionStatus.Success,
                    OrderReference = "ORDER_001",
                    TransactionDate = DateTime.UtcNow.AddDays(-2)
                },
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    BankId = "Garanti",
                    TotalAmount = 2000.00m,
                    NetAmount = 2000.00m,
                    Status = TransactionStatus.Fail,
                    OrderReference = "ORDER_002",
                    TransactionDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>()))
                .ReturnsAsync(transactions);

            var transactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactions[0].Id,
                    TransactionType = TransactionType.Sale,
                    Status = TransactionStatus.Success,
                    Amount = 1000.00m
                },
                new TransactionDetail
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactions[1].Id,
                    TransactionType = TransactionType.Sale,
                    Status = TransactionStatus.Fail,
                    Amount = 2000.00m
                }
            };

            _transactionDetailRepositoryMock.Setup(repo => repo.GetByTransactionIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(transactionDetails);

            // Act
            var result = await _searchService.SearchAsync(new SearchPaymentQuery());

            // Assert
            result.Should().HaveCount(2);
            result.First().TransactionId.Should().Be(transactions[0].Id);
            result.First().TransactionDetails.Should().HaveCount(1);
            result.First().TransactionDetails.First().Amount.Should().Be(1000.00m);
            result.Last().TransactionId.Should().Be(transactions[1].Id);
            result.Last().TransactionDetails.Should().HaveCount(1);
            result.Last().TransactionDetails.First().Amount.Should().Be(2000.00m);

            _transactionRepositoryMock.Verify(repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>()),
                Times.Once);
            _transactionDetailRepositoryMock.Verify(
                repo => repo.GetByTransactionIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmpty_WhenNoTransactionsMatch()
        {
            // Arrange
            var transactions = new List<Transaction>();

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>())).ReturnsAsync(transactions);

            // Act
            var result = await _searchService.SearchAsync(new SearchPaymentQuery
            {
                BankId="NonExistentBank",
                Status=TransactionStatus.Success, 
                OrderReference="ORDER_X", 
                StartDate=DateTime.UtcNow.AddDays(-10), 
                EndDate=DateTime.UtcNow,
            });

            // Assert
            result.Should().BeEmpty();

            _transactionRepositoryMock.Verify(
                repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>()), Times.Once);
            _transactionDetailRepositoryMock.Verify(
                repo => repo.GetByTransactionIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        }

        [Fact]
        public async Task SearchAsync_ShouldApplyAllFilters()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    BankId = "YapiKredi",
                    TotalAmount = 1500.00m,
                    NetAmount = 1500.00m,
                    Status = TransactionStatus.Success,
                    OrderReference = "ORDER_003",
                    TransactionDate = new DateTime(2023, 03, 10)
                }
            };
            _transactionRepositoryMock.Setup(repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>()))
                .ReturnsAsync(transactions);

            var transactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactions[0].Id,
                    TransactionType = TransactionType.Sale,
                    Status = TransactionStatus.Success,
                    Amount = 1500.00m
                }
            };

            _transactionDetailRepositoryMock
                .Setup(repo => repo.GetByTransactionIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(transactionDetails);

            // Act
            var result = await _searchService.SearchAsync(new SearchPaymentQuery
            {
                BankId = "YapiKredi",
                Status = TransactionStatus.Success,
                OrderReference = "ORDER_003",
                StartDate = new DateTime(2023, 03, 01),
                EndDate = new DateTime(2023, 03, 31),
            });

            // Assert
            result.Should().HaveCount(1);
            var report = result.First();
            report.BankId.Should().Be("YapiKredi");
            report.Status.Should().Be(TransactionStatus.Success);
            report.OrderReference.Should().Be("ORDER_003");
            report.TransactionDate.Should().Be(new DateTime(2023, 03, 10));
            report.TransactionDetails.Should().HaveCount(1);
            report.TransactionDetails.First().Amount.Should().Be(1500.00m);

            _transactionRepositoryMock.Verify(
                repo => repo.GetTransactionsByQuery(It.IsAny<SearchPaymentQuery>()), Times.Once);
            _transactionDetailRepositoryMock.Verify(
                repo => repo.GetByTransactionIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }
    }
}