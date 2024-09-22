namespace Payment.Application.Exceptions;

public class BankNotFoundException: Exception
{
    public BankNotFoundException(string message) : base(message)
    {
    }
}