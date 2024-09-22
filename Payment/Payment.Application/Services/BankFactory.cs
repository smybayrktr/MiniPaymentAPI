using Payment.Application.Exceptions;
using Payment.Application.Interfaces;

namespace Payment.Application.Services;

public class BankFactory : IBankFactory
{
    private readonly AkbankService _akbankService;
    private readonly GarantiService _garantiService;
    private readonly YapiKrediService _yapiKrediService;

    public BankFactory(
        AkbankService akbankService,
        GarantiService garantiService,
        YapiKrediService yapiKrediService)
    {
        _akbankService = akbankService;
        _garantiService = garantiService;
        _yapiKrediService = yapiKrediService;
    }

    public IBankService GetBankService(string bankId)
    {
        return bankId.ToLower().Trim() switch
        {
            "akbank" => _akbankService,
            "garanti" => _garantiService,
            "yapikredi" => _yapiKrediService,
            "yapıkredi" => _yapiKrediService,
            _ => throw new BankNotFoundException("Invalid Bank Id"),
        };
    }
}
