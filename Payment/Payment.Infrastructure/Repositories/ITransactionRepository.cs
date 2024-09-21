using Payment.Contracts.Queries;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task AddAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetTransactionsByQuery(SearchPaymentQuery searchPaymentQuery);
    Task UpdateAsync(Transaction transaction);
}
