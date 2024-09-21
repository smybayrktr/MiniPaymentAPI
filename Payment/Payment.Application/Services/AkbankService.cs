using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public class AkbankService : BaseBankService
{
    public AkbankService(ITransactionRepository transactionRepository, ITransactionDetailRepository transactionDetailRepository)
            : base(transactionRepository, transactionDetailRepository)
    {
    }

}
