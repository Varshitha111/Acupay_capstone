using Microsoft.Data.SqlClient;

namespace AcuPay_Capstone.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string? _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public SqlConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException(
                "The connection string is not set. Follow the 'Setting your connection string' steps in the README.");

        return new SqlConnection(_connectionString);
    }
}
