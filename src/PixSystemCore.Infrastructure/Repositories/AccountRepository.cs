using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PixSystemCore.Domain.Entities;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly UnitOfWork _unitOfWork;

    public AccountRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = (UnitOfWork)unitOfWork;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        const string query = @"
        INSERT INTO Accounts (Id, HolderName, PixKey, PixKeyType, Balance, CreatedAt)
        VALUES (@Id, @HolderName, @PixKey, @PixKeyType, @Balance, @CreatedAt);";

        var connection = _unitOfWork.GetConnection();
        var transaction = _unitOfWork.GetCurrentTransaction();

        await using var command = new SqlCommand(query, connection, transaction);

        command.Parameters.AddWithValue("@Id", account.Id);
        command.Parameters.AddWithValue("@HolderName", account.HolderName);
        command.Parameters.AddWithValue("@PixKey", account.PixKey);
        command.Parameters.AddWithValue("@PixKeyType", (int)account.KeyType);
        command.Parameters.AddWithValue("@Balance", account.Balance);
        command.Parameters.AddWithValue("@CreatedAt", account.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM Accounts WHERE Id = @Id;";

        var connection = _unitOfWork.GetConnection();
        var transaction = _unitOfWork.GetCurrentTransaction();

        await using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapReaderToAccount(reader);
        }

        return null;
    }

    public async Task<Account?> GetByPixKeyAsync(string pixKey, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM Accounts WHERE PixKey = @PixKey;";

        var connection = _unitOfWork.GetConnection();
        var transaction = _unitOfWork.GetCurrentTransaction();

        await using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@PixKey", pixKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapReaderToAccount(reader);
        }

        return null;
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        const string query = "UPDATE Accounts SET Balance = @Balance WHERE Id = @Id;";

        var connection = _unitOfWork.GetConnection();
        var transaction = _unitOfWork.GetCurrentTransaction();

        await using var command = new SqlCommand(query, connection, transaction);

        command.Parameters.AddWithValue("@Balance", account.Balance);
        command.Parameters.AddWithValue("@Id", account.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private Account MapReaderToAccount(SqlDataReader reader)
    {
        var account = (Account)FormatterServices.GetUninitializedObject(typeof(Account));

        SetHiddenProperty(account, nameof(Account.Id), reader.GetGuid(reader.GetOrdinal("Id")));
        SetHiddenProperty(account, nameof(Account.HolderName), reader.GetString(reader.GetOrdinal("HolderName")));
        SetHiddenProperty(account, nameof(Account.PixKey), reader.GetString(reader.GetOrdinal("PixKey")));
        SetHiddenProperty(account, nameof(Account.KeyType), reader.GetInt32(reader.GetOrdinal("PixKeyType")));
        SetHiddenProperty(account, nameof(Account.Balance), reader.GetDecimal(reader.GetOrdinal("Balance")));
        SetHiddenProperty(account, nameof(Account.CreatedAt), reader.GetDateTime(reader.GetOrdinal("CreatedAt")));

        return account;
    }

    private void SetHiddenProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value, null);
        }
        else
        {
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}