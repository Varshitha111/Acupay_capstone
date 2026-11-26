namespace AcuPay_Capstone.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var scope = new Dictionary<string, object> { ["CorrelationId"] = correlationId };
        using (_logger.BeginScope(scope))
        {
            _logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
        }
    }
}
