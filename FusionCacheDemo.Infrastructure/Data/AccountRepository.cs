using Dapper;
using FusionCacheDemo.Domain.Entities;
using FusionCacheDemo.Domain.Interfaces;

namespace FusionCacheDemo.Infrastructure.Data;

public class AccountRepository(ISqlConnectionFactory connectionFactory) : IAccountRepository
{
    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<Account>("SELECT Id, Name, Balance FROM Accounts");
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Account>(
            "SELECT Id, Name, Balance FROM Accounts WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Account entity)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Accounts (Name, Balance) VALUES (@Name, @Balance);
              SELECT CAST(SCOPE_IDENTITY() AS INT)", entity);
    }

    public async Task<bool> UpdateAsync(Account entity)
    {
        using var connection = connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "UPDATE Accounts SET Name = @Name, Balance = @Balance WHERE Id = @Id", entity);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "DELETE FROM Accounts WHERE Id = @Id", new { Id = id });
        return affected > 0;
    }
}
