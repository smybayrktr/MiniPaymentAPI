using Microsoft.EntityFrameworkCore;
using Payment.Contracts.Queries;
using Payment.Domain.Entities;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PaymentDbContext _context;

    public TransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<Transaction> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByQuery(SearchPaymentQuery searchPaymentQuery)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrEmpty(searchPaymentQuery.BankId))
            query = query.Where(t => t.BankId == searchPaymentQuery.BankId);

        if (searchPaymentQuery.Status.HasValue)
            query = query.Where(t => t.Status == searchPaymentQuery.Status.Value);

        if (!string.IsNullOrEmpty(searchPaymentQuery.OrderReference))
            query = query.Where(t => t.OrderReference == searchPaymentQuery.OrderReference);

        if (searchPaymentQuery.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= searchPaymentQuery.StartDate.Value);

        if (searchPaymentQuery.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= searchPaymentQuery.EndDate.Value);

        return await query.ToListAsync();
    }
}