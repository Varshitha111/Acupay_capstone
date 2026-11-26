using AcuPay_Capstone.Data;
using AcuPay_Capstone.Domain;
using AcuPay_Capstone.Dtos;
using AcuPay_Capstone.Exceptions;
using Microsoft.Data.SqlClient;

namespace AcuPay_Capstone.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        int statusCode;
        string code;
        string message;

        if (ex is AcuPayException known)
        {
            statusCode = GetStatusCode(known);
            code = known.Code;
            message = known.Message;
            _logger.LogWarning("Request rejected with {Code}: {Message}", code, message);
        }
        else if (ex is SqlException)
        {
            statusCode = StatusCodes.Status503ServiceUnavailable;
            code = "DATABASE_UNAVAILABLE";
            message = "The database is not available right now. Please try again shortly.";
            _logger.LogError(ex, "The database could not be reached or returned an error");
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            code = "INTERNAL_ERROR";
            message = "Something went wrong. Please quote the correlation ID when you report it.";
            _logger.LogError(ex, "Unhandled error");
        }

        string correlationId = context.Items["CorrelationId"]?.ToString() ?? "";
        var body = new ErrorResponse(new ErrorBody(code, message, correlationId));

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(body);
    }

    private static int GetStatusCode(AcuPayException ex)
    {
        return ex switch
        {
            ForbiddenRoleException => StatusCodes.Status403Forbidden,
            SelfApprovalBlockedException => StatusCodes.Status403Forbidden,
            InvalidStateException => StatusCodes.Status409Conflict,
            DuplicateInvoiceException => StatusCodes.Status409Conflict,
            ReasonRequiredException => StatusCodes.Status400BadRequest,
            ReferenceNotFoundException => StatusCodes.Status400BadRequest,
            InvalidRequestException => StatusCodes.Status400BadRequest,
            UserHeaderRequiredException => StatusCodes.Status400BadRequest,
            UserNotFoundException => StatusCodes.Status401Unauthorized,
            InvoiceNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
