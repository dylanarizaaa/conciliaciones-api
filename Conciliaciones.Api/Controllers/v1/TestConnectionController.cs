using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conciliaciones.Api.Controllers.v1
{
    /// <summary>
    /// Diagnósticos DocumentDB. Solo disponibles en Development para evitar fuga de catálogo/errores.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class TestConnectionController : BaseApiController
    {
        private readonly IDatabaseProbe _databaseProbe;
        private readonly IHostEnvironment _environment;

        public TestConnectionController(IDatabaseProbe databaseProbe, IHostEnvironment environment)
        {
            _databaseProbe = databaseProbe ?? throw new ArgumentNullException(nameof(databaseProbe));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpGet("check-mongo")]
        public async Task<IActionResult> CheckMongoConnection(CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            DatabasePingResult result = await _databaseProbe.PingAsync(cancellationToken);
            if (!result.Ok)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = result.Status
                });
            }

            return Ok(new
            {
                Status = result.Status,
                DatabaseName = result.DatabaseName,
                CollectionCount = result.Collections?.Count ?? 0
            });
        }

        [HttpGet("check-databases")]
        public async Task<IActionResult> CheckDatabases(CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            DatabaseCatalogResult result = await _databaseProbe.ListDatabasesAsync(cancellationToken);
            if (!result.Ok)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = result.Status
                });
            }

            return Ok(new
            {
                DatabaseCount = result.Databases?.Count ?? 0,
                BaseDeDatosActualEnConfig = result.CurrentDatabase
            });
        }
    }
}
