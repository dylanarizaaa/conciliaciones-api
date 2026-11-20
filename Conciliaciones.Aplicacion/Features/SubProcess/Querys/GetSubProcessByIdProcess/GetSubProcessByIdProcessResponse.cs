using Conciliaciones.Dominio.DTOs.Response.SubProcess;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;

public record GetSubProcessByIdProcessResponse(
    decimal totalAfectado,
    long casosTotales,
    List<SubProcessInfoDto> data
    );
