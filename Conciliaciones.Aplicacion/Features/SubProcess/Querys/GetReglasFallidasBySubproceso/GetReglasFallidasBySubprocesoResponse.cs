using Conciliaciones.Dominio.DTOs.Response.SubProcess;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;

public record GetReglasFallidasBySubprocesoResponse(
    long casosTotales,
    decimal montoAfectado,
    int pageNumber,
    int pageSize,
    long totalPages,
    List<FailedRuleDto> data
    );


