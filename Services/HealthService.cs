using AcuPay_Capstone.Data;

namespace AcuPay_Capstone.Services;

public class HealthService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<HealthService> _logger;

    public HealthService(IDbConnectionFactory connectionFactory, ILogger<HealthService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<bool> IsDatabaseReachableAsync()
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed: the database did not answer");
            return false;
        }
    }
}
