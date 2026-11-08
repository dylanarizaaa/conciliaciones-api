using Conciliaciones.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Conciliaciones.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConciliacionesController : ControllerBase
{
    // ---- Datos de ejemplo en memoria ----
    private static readonly Guid ProyectoPortal = Guid.Parse("b67a7c53-68cd-4c20-ba6c-6165edfd7814");
    private static readonly Guid ProcesoVentas = Guid.Parse("09f12723-a85a-4d28-9520-25fa6fee7b04");
    private static readonly Guid SubProcesoVentasCredito = Guid.Parse("aafa05df-cf84-4365-a5d0-2eee83c14a15");

    private static readonly List<ProyectoDto> Proyectos =
    [
        new() { ProyectoId = ProyectoPortal, NombreProyecto = "Portal", Monto = 0, Cantidad = 1 }
    ];

    private static readonly List<ProcesoDto> Procesos =
    [
        new() { ProyectoId = ProyectoPortal, ProcesoId = ProcesoVentas, NombreProceso = "Ventas", Monto = 0, Cantidad = 1 }
    ];

    private static readonly List<SubProcesoDto> SubProcesos =
    [
        new() { ProcesoId = ProcesoVentas, SubProcesoId = SubProcesoVentasCredito, NombreSubProceso = "ventas a creditooo", Monto = 0, Cantidad = 1 }
    ];

    private static readonly List<ReglaDto> Reglas =
    [
        new() { SubProcesoId = SubProcesoVentasCredito, Nombre = "ValidacionMontoExacto", Casos = 1, Monto = 8908 }
    ];

    /// <summary>Obtiene la lista de proyectos.</summary>
    [HttpGet("proyectos")]
    [ProducesResponseType<RespuestaApi<ResultadoLista<ProyectoDto>>>(StatusCodes.Status200OK)]
    public ActionResult<RespuestaApi<ResultadoLista<ProyectoDto>>> ObtenerProyectos()
    {
        var resultado = new ResultadoLista<ProyectoDto> { Data = Proyectos };
        return Ok(RespuestaApi<ResultadoLista<ProyectoDto>>.Ok(resultado));
    }

    /// <summary>Obtiene los procesos de un proyecto.</summary>
    [HttpGet("procesos")]
    [ProducesResponseType<RespuestaApi<ResultadoListaConTotales<ProcesoDto>>>(StatusCodes.Status200OK)]
    public ActionResult<RespuestaApi<ResultadoListaConTotales<ProcesoDto>>> ObtenerProcesos(
        [FromQuery, BindRequired] Guid proyectoId)
    {
        var procesos = Procesos.Where(p => p.ProyectoId == proyectoId).ToList();
        var resultado = new ResultadoListaConTotales<ProcesoDto>
        {
            TotalAfectado = procesos.Sum(p => p.Monto),
            CasosTotales = procesos.Sum(p => p.Cantidad),
            Data = procesos
        };
        return Ok(RespuestaApi<ResultadoListaConTotales<ProcesoDto>>.Ok(resultado));
    }

    /// <summary>Obtiene los subprocesos de un proceso.</summary>
    [HttpGet("subprocesos")]
    [ProducesResponseType<RespuestaApi<ResultadoListaConTotales<SubProcesoDto>>>(StatusCodes.Status200OK)]
    public ActionResult<RespuestaApi<ResultadoListaConTotales<SubProcesoDto>>> ObtenerSubProcesos(
        [FromQuery(Name = "IdProcess"), BindRequired] Guid idProcess)
    {
        var subProcesos = SubProcesos.Where(s => s.ProcesoId == idProcess).ToList();
        var resultado = new ResultadoListaConTotales<SubProcesoDto>
        {
            TotalAfectado = subProcesos.Sum(s => s.Monto),
            CasosTotales = subProcesos.Sum(s => s.Cantidad),
            Data = subProcesos
        };
        return Ok(RespuestaApi<ResultadoListaConTotales<SubProcesoDto>>.Ok(resultado));
    }

    /// <summary>Obtiene, paginadas, las reglas de un subproceso.</summary>
    [HttpGet("subprocesos/reglas/{subProcesoId:guid}")]
    [ProducesResponseType<RespuestaApi<ResultadoDato<ResultadoPaginado<ReglaDto>>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RespuestaApi<ResultadoDato<ResultadoPaginado<ReglaDto>>>>(StatusCodes.Status400BadRequest)]
    public ActionResult<RespuestaApi<ResultadoDato<ResultadoPaginado<ReglaDto>>>> ObtenerReglasSubProceso(
        Guid subProcesoId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest(RespuestaApi<ResultadoDato<ResultadoPaginado<ReglaDto>>>.Error(
                StatusCodes.Status400BadRequest,
                "Parámetros de paginación inválidos.",
                "pageNumber y pageSize deben ser mayores o iguales a 1."));
        }

        var reglas = Reglas.Where(r => r.SubProcesoId == subProcesoId).ToList();
        var pagina = new ResultadoPaginado<ReglaDto>
        {
            CasosTotales = reglas.Sum(r => r.Casos),
            MontoAfectado = reglas.Sum(r => r.Monto),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(reglas.Count / (double)pageSize),
            Data = reglas.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
        var resultado = new ResultadoDato<ResultadoPaginado<ReglaDto>> { Data = pagina };
        return Ok(RespuestaApi<ResultadoDato<ResultadoPaginado<ReglaDto>>>.Ok(resultado));
    }
}
