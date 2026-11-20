namespace Conciliaciones.Dominio.DTOs.Response.Process;

public record ProcesstSummaryDto(
    string ProcesoId,
    string NombreProceso,
    decimal Monto,
    long Cantidad
    );

