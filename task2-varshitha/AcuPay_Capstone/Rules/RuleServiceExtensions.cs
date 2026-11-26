namespace AcuPay_Capstone.Rules;

public static class RuleServiceExtensions
{
    public static IServiceCollection AddRules(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RuleSettings");
        if (!section.Exists())
            throw new InvalidOperationException("The RuleSettings section is missing from appsettings.json.");

        decimal tolerancePercent = section.GetValue<decimal>("PriceTolerancePercent");
        decimal autoApproveLimit = section.GetValue<decimal>("AutoApproveLimit");
        int minReasonLength = section.GetValue<int>("MinReasonLength");

        services.AddSingleton(new MatchingRules(tolerancePercent, autoApproveLimit));
        services.AddSingleton(new ApprovalRules(minReasonLength));
        return services;
    }
}
