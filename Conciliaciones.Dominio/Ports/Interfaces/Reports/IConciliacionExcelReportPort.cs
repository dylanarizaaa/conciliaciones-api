using Conciliaciones.Dominio.DTOs.Response.Reports;

namespace Conciliaciones.Dominio.Ports.Interfaces.Reports;

/// <summary>
/// Genera el workbook Excel (opcionalmente cifrado OLE2) a partir del dataset de export.
/// </summary>
public interface IConciliacionExcelReportPort
{
    byte[] GenerarExcelConciliaciones(ConciliacionExportDataDto data);
}
