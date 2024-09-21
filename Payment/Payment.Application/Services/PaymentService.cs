using Payment.Application.Interfaces;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBankFactory _bankFactory;

    public PaymentService(ITransactionRepository transactionRepository, IBankFactory bankFactory)
    {
        _transactionRepository = transactionRepository;
        _bankFactory = bankFactory;
    }

    public async Task<Transaction> PayAsync(PayTransactionCommand command)
    {
        var bankService = _bankFactory.GetBankService(command.BankId);
        return await bankService.PayAsync(command);
    }

    public async Task<Transaction> CancelAsync(CancelTransactionCommand command)
    {
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);
        if (transaction == null)
            throw new Exception("Transaction not found");

        var bankService = _bankFactory.GetBankService(transaction.BankId);
        return await bankService.CancelAsync(command.TransactionId);
    }

    public async Task<Transaction> RefundAsync(RefundTransactionCommand command)
    {
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);
        if (transaction == null)
            throw new Exception("Transaction not found");

        var bankService = _bankFactory.GetBankService(transaction.BankId);
        return await bankService.RefundAsync(command.TransactionId);
    }
}
