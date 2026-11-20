namespace Conciliaciones.Dominio.DTOs.Response.SubProcess;

public record SubProcessInfoDto
(
    string SubProcesoId,
    string NombreSubProceso,
    decimal Monto,
    long Cantidad
);

