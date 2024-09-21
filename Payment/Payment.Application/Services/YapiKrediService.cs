using Payment.Infrastructure.Repositories;

namespace Payment.Application.Services;

public class YapiKrediService : BaseBankService
{
    public YapiKrediService(ITransactionRepository transactionRepository, ITransactionDetailRepository transactionDetailRepository)
            : base(transactionRepository, transactionDetailRepository)
    {
    }
}
