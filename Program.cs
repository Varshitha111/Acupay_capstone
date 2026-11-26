using AcuPay_Capstone.Data;
using AcuPay_Capstone.Dtos;
using AcuPay_Capstone.Middleware;
using AcuPay_Capstone.Rules;
using AcuPay_Capstone.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The request is not valid." : error.ErrorMessage);

            string correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "";
            var body = new ErrorResponse(new ErrorBody("VALIDATION_ERROR", string.Join(" ", messages), correlationId));
            return new BadRequestObjectResult(body);
        };
    });

builder.Services.AddDataAccess();
builder.Services.AddRules(builder.Configuration);
builder.Services.AddScoped<HealthService>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();

app.Run();
