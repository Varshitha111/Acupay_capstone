using AcuPay_Capstone.Dtos;
using AcuPay_Capstone.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcuPay_Capstone.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthService _healthService;
    private readonly IWebHostEnvironment _environment;

    public HealthController(HealthService healthService, IWebHostEnvironment environment)
    {
        _healthService = healthService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        bool reachable = await _healthService.IsDatabaseReachableAsync();

        var body = new HealthResponse
        {
            Status = reachable ? "Healthy" : "Unhealthy",
            Environment = _environment.EnvironmentName,
            Database = reachable ? "Reachable" : "Unreachable"
        };

        return StatusCode(reachable ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable, body);
    }
}
