using Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion;
using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conciliaciones.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "RequireRole")]
    public class AutomatizacionesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AutomatizacionesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        public async Task<ActionResult<Answers<CreateAutomatizacionResponse>>> Create(
            [FromBody] CreateAutomatizacionRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateAutomatizacionCommand command = new(request);
            Answers<CreateAutomatizacionResponse> result = await _mediator.Send(command, cancellationToken);
            return StatusCode((int)result.CodigoEstado, result);
        }
    }
}
