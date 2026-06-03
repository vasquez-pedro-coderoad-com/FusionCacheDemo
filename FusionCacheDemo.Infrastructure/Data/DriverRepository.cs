using Dapper;
using FusionCacheDemo.Domain.Entities;
using FusionCacheDemo.Domain.Interfaces;

namespace FusionCacheDemo.Infrastructure.Data;

public class DriverRepository(ISqlConnectionFactory connectionFactory) : IDriverRepository
{
    public async Task<IEnumerable<Driver>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<Driver>(
            "SELECT Id, Name, TruckId, IsActive FROM Drivers");
    }

    public async Task<IEnumerable<Driver>> GetActiveDriversAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<Driver>(
            "SELECT Id, Name, TruckId, IsActive FROM Drivers WHERE IsActive = 1");
    }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Driver>(
            "SELECT Id, Name, TruckId, IsActive FROM Drivers WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Driver entity)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Drivers (Name, TruckId, IsActive) VALUES (@Name, @TruckId, @IsActive);
              SELECT CAST(SCOPE_IDENTITY() AS INT)", entity);
    }

    public async Task<bool> UpdateAsync(Driver entity)
    {
        using var connection = connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(
            @"UPDATE Drivers SET Name = @Name, TruckId = @TruckId, IsActive = @IsActive
              WHERE Id = @Id", entity);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "DELETE FROM Drivers WHERE Id = @Id", new { Id = id });
        return affected > 0;
    }
}
