using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conciliaciones.Api.Controllers.v1
{
    [ApiController]
    [AllowAnonymous]
    public class HealthCheckController : BaseApiController
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(
            HealthCheckService healthCheckService,
            ILogger<HealthCheckController> logger)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("HealthCheck")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("[Entrada] Healthcheck Funcionando");
            return Ok("Funcionando.");
        }

        [HttpGet("health/ready")]
        public async Task<IActionResult> Ready()
        {
            HealthReport report = await _healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("ready"));

            if (report.Status == HealthStatus.Healthy)
            {
                return Ok(MapResponse(report));
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, MapResponse(report));
        }

        [HttpGet("health/live")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Live()
        {
            return Ok("Alive");
        }

        private static object MapResponse(HealthReport report)
        {
            return new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };
        }
    }
}