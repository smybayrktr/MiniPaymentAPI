using Payment.Domain.Entities;

namespace Payment.Infrastructure.Repositories;

public interface ITransactionDetailRepository
{
    Task AddAsync(TransactionDetail transactionDetail);
    Task<TransactionDetail> GetByIdAsync(Guid id);
    Task UpdateAsync(TransactionDetail transactionDetail);
    Task<IEnumerable<TransactionDetail>> GetByTransactionIdsAsync(IEnumerable<Guid> transactionIds);
}
