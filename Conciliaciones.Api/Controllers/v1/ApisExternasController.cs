using Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conciliaciones.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "RequireRole")]
    public class ApisExternasController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ApisExternasController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Único GET: parametrizaciones + parámetros + reglas, filtrado por jerarquía.
        /// </summary>
        [HttpGet("parametrizaciones")]
        public async Task<ActionResult<Answers<GetParameterizationsByClientResponse>>> GetParametrizaciones(
            [FromQuery] string? proyectoId,
            [FromQuery] string? procesoId,
            [FromQuery] string? subProcesoId,
            CancellationToken cancellationToken = default)
        {
            GetParameterizationsByClientQuery query = new(proyectoId, procesoId, subProcesoId);
            Answers<GetParameterizationsByClientResponse> result = await _mediator.Send(query, cancellationToken);
            return StatusCode((int)result.CodigoEstado, result);
        }
    }
}
