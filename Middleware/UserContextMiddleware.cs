using AcuPay_Capstone.Data;
using AcuPay_Capstone.Exceptions;

namespace AcuPay_Capstone.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IInvoiceRepository repository)
    {
        PathString path = context.Request.Path;
        bool needsUser = path.StartsWithSegments("/api") && !path.StartsWithSegments("/api/health");

        if (needsUser)
        {
            string? username = context.Request.Headers["X-User"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(username))
                throw new UserHeaderRequiredException();

            AppUser? user = await repository.GetUserByUsernameAsync(username.Trim());
            if (user == null)
                throw new UserNotFoundException(username.Trim());

            context.Items["User"] = user;
        }

        await _next(context);
    }
}

public static class HttpContextUserExtensions
{
    public static AppUser GetCurrentUser(this HttpContext context)
    {
        return (AppUser)context.Items["User"]!;
    }
}
