namespace Payment.Application.Exceptions;

public class TransactionNotFoundException: Exception
{
    public TransactionNotFoundException(string message) : base(message)
    {
    }
}