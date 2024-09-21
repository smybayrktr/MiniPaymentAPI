namespace Payment.Application.Interfaces;

public interface IBankFactory
{
    IBankService GetBankService(string bankId);
}
