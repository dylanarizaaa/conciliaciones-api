using Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conciliaciones.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "RequireRole")]

    public class InformesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public InformesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("export-excel")]
        [ProducesResponseType(typeof(Answers<ExcelDataReportsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Answers<ExcelDataReportsResponse>>> ExportExcel([FromQuery] string reglaId, CancellationToken cancellationToken = default)
        {
            ExcelDataReportsQuery query = new(reglaId);
            Answers<ExcelDataReportsResponse> result = await _mediator.Send(query, cancellationToken);
            return StatusCode((int)result.CodigoEstado, result);
        }

    }
}
