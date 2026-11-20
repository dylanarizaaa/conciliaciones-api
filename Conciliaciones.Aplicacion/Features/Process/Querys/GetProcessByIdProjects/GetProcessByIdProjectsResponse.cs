using Conciliaciones.Dominio.DTOs.Response.Process;

namespace Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;

public record GetProcessByIdProjectsResponse(
    decimal totalAfectado,
    long casosTotales,
    List<ProcesstSummaryDto> data
);

