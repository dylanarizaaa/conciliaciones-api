using Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull;
using Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla;
using Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;
using Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conciliaciones.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "RequireRole")]

    public class ConciliacionesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ConciliacionesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [HttpPost("crear-parametrizacion")]
        public async Task<ActionResult<Answers<CreateParametrizacionFullResponse>>> CreateParametrizacionFull(
        [FromBody] CreateParametrizacionRequest request,
        CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
            }

            CreateParametrizacionFullCommand command = new CreateParametrizacionFullCommand(request);
            Answers<CreateParametrizacionFullResponse> result = await _mediator.Send(command, cancellationToken);

            return StatusCode((int)result.CodigoEstado, result);
        }
        [HttpGet("proyectos")]
        [ProducesResponseType(typeof(GetResumenProyectosResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<Answers<GetResumenProyectosResponse>>> GetResumenProyectos(CancellationToken cancellationToken)
        {
            GetResumenProyectosQuery query = new GetResumenProyectosQuery();
            Answers<GetResumenProyectosResponse> result = await _mediator.Send(query, cancellationToken);

            return StatusCode((int)result.CodigoEstado, result);
        }

        [HttpGet("procesos")]
        [ProducesResponseType(typeof(GetProcessByIdProjectsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResumenProcesosByProyecto([FromQuery] string proyectoId, CancellationToken cancellationToken)
        {
            GetProcessByIdProjectsQuery query = new GetProcessByIdProjectsQuery(proyectoId);
            Answers<GetProcessByIdProjectsResponse> result = await _mediator.Send(query, cancellationToken);

            return StatusCode((int)result.CodigoEstado, result);
        }

        [HttpGet("subprocesos")]
        [ProducesResponseType(typeof(GetSubProcessByIdProcessResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResumenSubProcessByProcessId([FromQuery] string IdProcess, CancellationToken cancellationToken)
        {
            GetSubProcessByIdProcessQuery query = new GetSubProcessByIdProcessQuery(IdProcess);
            Answers<GetSubProcessByIdProcessResponse> result = await _mediator.Send(query, cancellationToken);

            return StatusCode((int)result.CodigoEstado, result);
        }

        [HttpGet("subprocesos/reglas")]
        public async Task<ActionResult<Answers<GetReglasFallidasBySubprocesoResponse>>> GetReglasFallidasBySubproceso([FromQuery] string subProcesoId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            GetReglasFallidasBySubprocesoQuery query = new GetReglasFallidasBySubprocesoQuery(subProcesoId, pageNumber, pageSize);
            Answers<GetReglasFallidasBySubprocesoResponse> result = await _mediator.Send(query, cancellationToken);
            return StatusCode((int)result.CodigoEstado, result);
        }


        [HttpGet("subprocesos-ui/plantilla")]
        public async Task<ActionResult<Answers<GetParametrizacionPlantillaResponse>>> GetParametrizacionPlantilla([FromQuery] string reglaId, [FromQuery] int tipoBlock, CancellationToken cancellationToken = default)
        {
            string typeBlock = tipoBlock == 1 ? "table" : tipoBlock == 2 ? "accordion" : throw new ArgumentException("El parámetro tipoBlock debe ser 1 (table) o 2 (chart).");

            GetParametrizacionPlantillaQuery query = new(reglaId, typeBlock);
            Answers<GetParametrizacionPlantillaResponse> result = await _mediator.Send(query, cancellationToken);
            if (result.Resultado == null)
            {
                return StatusCode((int)result.CodigoEstado, result);
            }
            return StatusCode((int)result.CodigoEstado, new
            {
                codigoEstado = (int)result.CodigoEstado,
                respuestaEstado = result.RespuestaEstado,
                mensaje = result.Mensaje,
                errores = result.Errores,
                mensajeError = result.MensajeError,
                resultado = result.Resultado.Plantilla
            });
        }

        [HttpGet("subprocesos-registros/reglas")]
        public async Task<ActionResult<Answers<GetConciliationRowsResponse>>> GetConciliationRows(
            [FromQuery] string reglaId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            GetConciliationRowsQuery query = new GetConciliationRowsQuery(reglaId, pageNumber, pageSize);
            Answers<GetConciliationRowsResponse> result = await _mediator.Send(query, cancellationToken);

            return StatusCode((int)result.CodigoEstado, result);
        }


        [HttpGet("fuentes-detalle")]
        public async Task<ActionResult<Answers<GetConciliationFuentesDetailResponse>>> GetFuentesDetalle(
            [FromQuery] string conciliacionId,
            CancellationToken cancellationToken)
        {
            GetConciliationFuentesDetailQuery query = new GetConciliationFuentesDetailQuery(conciliacionId);
            Answers<GetConciliationFuentesDetailResponse> result = await _mediator.Send(query, cancellationToken);
            return StatusCode((int)result.CodigoEstado, result);
        }
        [HttpPut("cambio-estado")]
        public async Task<ActionResult<Answers<UpdateConciliacionEstadoResponse>>> ChangeEstadoToPendiente([FromBody] UpdateConciliacionEstadoRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.ConciliacionId == null || !request.ConciliacionId.Any())
            {
                return BadRequest(new { mensaje = "Debe proporcionar al menos un ID de conciliación en el cuerpo de la petición." });
            }

            UpdateConciliacionEstadoCommand command = new UpdateConciliacionEstadoCommand(request.ConciliacionId);

            Answers<UpdateConciliacionEstadoResponse> response = await _mediator.Send(command, cancellationToken);

            return StatusCode((int)response.CodigoEstado, response);
        }
    }
}