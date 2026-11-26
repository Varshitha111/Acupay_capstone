namespace AcuPay_Capstone.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        return services;
    }
}
