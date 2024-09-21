using Payment.Domain.Entities;
using Payment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Payment.Infrastructure.Repositories;

public class TransactionDetailRepository : ITransactionDetailRepository
{
    private readonly PaymentDbContext _context;

    public TransactionDetailRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TransactionDetail transactionDetail)
    {
        await _context.TransactionDetails.AddAsync(transactionDetail);
        await _context.SaveChangesAsync();
    }

    public async Task<TransactionDetail> GetByIdAsync(Guid id)
    {
        return await _context.TransactionDetails.FirstOrDefaultAsync(td => td.Id == id);
    }

    public async Task UpdateAsync(TransactionDetail transactionDetail)
    {
        _context.TransactionDetails.Update(transactionDetail);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TransactionDetail>> GetByTransactionIdsAsync(IEnumerable<Guid> transactionIds)
    {
        return await _context.TransactionDetails
            .Where(td => transactionIds.Contains(td.TransactionId))
            .ToListAsync();
    }
}