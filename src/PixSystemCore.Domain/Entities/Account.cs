using System.Transactions;
using PixSystemCore.Domain.Enums;
using PixSystemCore.Domain.Exceptions;

namespace PixSystemCore.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string HolderName { get; private set; }
    public string PixKey { get; private set; }
    public PixKeyType KeyType { get; private set; }
    public decimal Balance { get; private set; }
    public decimal BlockedBalance { get; private set; }
    public decimal AvailableBalance => Balance - BlockedBalance;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { }

    public Account(string holderName, string pixKey, PixKeyType keyType)
    {
        Id = Guid.NewGuid();
        HolderName = holderName;
        PixKey = pixKey;
        KeyType = keyType;
        Balance = 1000.00m;
        BlockedBalance = 0;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void LockBalance(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor de bloqueio deve ser positivo");

        if (AvailableBalance < valor)
            throw new InsufficientBalanceException("Saldo disponível insuficiente para bloqueio");

        BlockedBalance += valor;
    }

    public void Debit(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor de débito deve ser positivo");

        if (BlockedBalance < valor)
            throw new DomainException("Saldo bloqueado insuficiente para débito");

        Balance -= valor;
        BlockedBalance -= valor;
    }

    public void Credit(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor de crédito deve ser positivo");

        Balance += valor;
    }

    public Transaction AddTransaction(Transaction transacao)
    {
        _transactions.Add(transacao);
        return transacao;
    }
}
