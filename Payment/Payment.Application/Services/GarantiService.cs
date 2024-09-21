using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public class GarantiService : BaseBankService
{
    public GarantiService(ITransactionRepository transactionRepository, ITransactionDetailRepository transactionDetailRepository)
            : base(transactionRepository, transactionDetailRepository)
    {
    }

}
