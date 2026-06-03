using System.Data;
using Microsoft.Data.SqlClient;

namespace FusionCacheDemo.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
