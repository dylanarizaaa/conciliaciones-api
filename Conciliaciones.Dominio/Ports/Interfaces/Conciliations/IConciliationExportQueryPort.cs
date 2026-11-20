using Conciliaciones.Dominio.DTOs.Response.Reports;

namespace Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

/// <summary>
/// Puerto de lectura dedicada a export (sin paginación). Misma clave que registros: reglaId.
/// </summary>
public interface IConciliationExportQueryPort
{
    Task<ConciliacionExportDataDto> GetExportDataAsync(
        string reglaId,
        CancellationToken cancellationToken);
}
