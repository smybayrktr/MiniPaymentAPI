using Payment.Application.Exceptions;
using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public abstract class BaseBankService : IBankService
{
    protected readonly ITransactionRepository _transactionRepository;
    protected readonly ITransactionDetailRepository _transactionDetailRepository;

    public BaseBankService(ITransactionRepository transactionRepository,
        ITransactionDetailRepository transactionDetailRepository)
    {
        _transactionRepository = transactionRepository;
        _transactionDetailRepository = transactionDetailRepository;
    }

    public virtual async Task<Transaction> PayAsync(PayTransactionCommand command)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            BankId = command.BankId,
            TotalAmount = command.TotalAmount,
            NetAmount = command.TotalAmount,
            Status = TransactionStatus.Success,
            OrderReference = command.OrderReference,
            TransactionDate = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction);
        
        var transactionDetail = new TransactionDetail
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            TransactionType = TransactionType.Sale,
            Status = TransactionStatus.Success,
            Amount = command.TotalAmount
        };

        await _transactionDetailRepository.AddAsync(transactionDetail);
        
        return transaction;
    }

    public virtual async Task<Transaction> CancelAsync(Transaction transaction)
    {
        if (transaction.TransactionDate.Date != DateTime.UtcNow.Date)
            throw new BusinessLogicException("Cancel operation is only allowed on the same day");

        transaction.NetAmount -= transaction.TotalAmount;
        transaction.Status = TransactionStatus.Success;

        await _transactionRepository.UpdateAsync(transaction);
        
        var transactionDetail = new TransactionDetail
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            TransactionType = TransactionType.Cancel,
            Status = TransactionStatus.Success,
            Amount = transaction.TotalAmount
        };

        await _transactionDetailRepository.AddAsync(transactionDetail);

        return transaction;
    }

    public virtual async Task<Transaction> RefundAsync(Transaction transaction)
    {
        if ((DateTime.UtcNow - transaction.TransactionDate).TotalDays < 1)
            throw new BusinessLogicException("Refund operation is allowed only after one day");

        transaction.NetAmount -= transaction.TotalAmount;
        transaction.Status = TransactionStatus.Success;

        await _transactionRepository.UpdateAsync(transaction);
        
        var transactionDetail = new TransactionDetail
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            TransactionType = TransactionType.Refund,
            Status = TransactionStatus.Success,
            Amount = transaction.TotalAmount
        };

        await _transactionDetailRepository.AddAsync(transactionDetail);

        return transaction;
    }
}