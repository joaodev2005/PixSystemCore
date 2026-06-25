namespace PixSystemCore.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) {}
}

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string message) : base(message) { }
}

public class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(string message) : base(message) { }
}

public class PixKeyNotFoundException : DomainException
{
    public PixKeyNotFoundException(string message) : base(message) { }
}
