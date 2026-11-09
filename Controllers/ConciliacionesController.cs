using System.Text.Json.Nodes;
using Conciliaciones.API.Models;
using Conciliaciones.API.Services;
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

    // ---- Pantalla Reglas: plantilla paramétrica + casos ----

    private const string ValidadoPorEjemplo = "Alexander Tipacoque";

    private ObjectResult ReglaNoEncontrada<T>(string regla) =>
        NotFound(RespuestaApi<T>.Error(StatusCodes.Status404NotFound,
            "Regla no encontrada.", $"No existe la regla '{regla}'."));

    private ObjectResult ParametroInvalido<T>(ParametroInvalidoException ex) =>
        BadRequest(RespuestaApi<T>.Error(StatusCodes.Status400BadRequest,
            "Parámetros inválidos.", ex.Message));

    /// <summary>Obtiene la plantilla (columnas, celdas, orden, acciones) de la tabla de una regla.</summary>
    [HttpGet("subprocesos/{subProcesoId:guid}/reglas/{regla}/plantilla")]
    [ProducesResponseType<RespuestaApi<JsonObject>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RespuestaApi<JsonObject>>(StatusCodes.Status404NotFound)]
    public ActionResult<RespuestaApi<JsonObject>> ObtenerPlantillaRegla(Guid subProcesoId, string regla)
    {
        if (!ReglasCasosStore.ExisteRegla(regla)) return ReglaNoEncontrada<JsonObject>(regla);
        return Ok(RespuestaApi<JsonObject>.Ok(ReglasCasosStore.ObtenerPlantilla(regla)));
    }

    /// <summary>Obtiene, paginados y ordenados, los casos con diferencias de una regla.</summary>
    [HttpGet("subprocesos/{subProcesoId:guid}/reglas/{regla}/casos")]
    [ProducesResponseType<RespuestaApi<PaginaCasos>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RespuestaApi<PaginaCasos>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<RespuestaApi<PaginaCasos>>(StatusCodes.Status404NotFound)]
    public ActionResult<RespuestaApi<PaginaCasos>> ObtenerCasosRegla(
        Guid subProcesoId,
        string regla,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string[]? sort = null)
    {
        if (!ReglasCasosStore.ExisteRegla(regla)) return ReglaNoEncontrada<PaginaCasos>(regla);
        try
        {
            return Ok(RespuestaApi<PaginaCasos>.Ok(
                ReglasCasosStore.ObtenerPagina(regla, pageNumber, pageSize, sort ?? [])));
        }
        catch (ParametroInvalidoException ex)
        {
            return ParametroInvalido<PaginaCasos>(ex);
        }
    }

    /// <summary>Descarga en CSV los casos de una regla con el orden indicado.</summary>
    [HttpGet("subprocesos/{subProcesoId:guid}/reglas/{regla}/casos/reporte")]
    [Produces("text/csv", "application/json")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType<RespuestaApi<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<RespuestaApi<object>>(StatusCodes.Status404NotFound)]
    public IActionResult DescargarReporteCasos(Guid subProcesoId, string regla, [FromQuery] string[]? sort = null)
    {
        if (!ReglasCasosStore.ExisteRegla(regla)) return ReglaNoEncontrada<object>(regla);
        try
        {
            var csv = ReglasCasosStore.GenerarReporteCsv(regla, sort ?? []);
            return File(csv, "text/csv", $"diferencias-{regla}.csv");
        }
        catch (ParametroInvalidoException ex)
        {
            return ParametroInvalido<object>(ex);
        }
    }

    /// <summary>Resuelve los casos seleccionados. Por ahora solo confirma con 200.</summary>
    [HttpPost("subprocesos/{subProcesoId:guid}/reglas/{regla}/casos/resolver")]
    [ProducesResponseType<RespuestaApi<ResolverCasosResultado>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RespuestaApi<ResolverCasosResultado>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<RespuestaApi<ResolverCasosResultado>>(StatusCodes.Status404NotFound)]
    public ActionResult<RespuestaApi<ResolverCasosResultado>> ResolverCasos(
        Guid subProcesoId, string regla, [FromBody] ResolverCasosRequest solicitud)
    {
        if (!ReglasCasosStore.ExisteRegla(regla)) return ReglaNoEncontrada<ResolverCasosResultado>(regla);

        var ids = solicitud.Ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return BadRequest(RespuestaApi<ResolverCasosResultado>.Error(
                StatusCodes.Status400BadRequest, "Solicitud inválida.", "Debe enviar al menos un id en 'ids'."));
        }

        var resultado = new ResolverCasosResultado
        {
            Procesados = ids.Count,
            Pendientes = 0,
            ValidadoPor = ValidadoPorEjemplo,
            FechaValidacion = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)) // hora de Colombia
        };
        return Ok(RespuestaApi<ResolverCasosResultado>.Ok(resultado));
    }
}
