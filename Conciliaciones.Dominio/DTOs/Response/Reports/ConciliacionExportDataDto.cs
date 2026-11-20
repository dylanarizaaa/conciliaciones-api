namespace Conciliaciones.Dominio.DTOs.Response.Reports;

/// <summary>
/// Dataset listo para Excel: columnas UI (sin accion) + filas alineadas por fuenteId.
/// DEBUG: inspeccionar Columns[].RowKey vs Keys de cada Rows[i].
/// </summary>
public sealed record ConciliacionExportColumnDto(
    string Header,
    string RowKey,
    string? CellType,
    string? FuenteId,
    string? PropertyName,
    bool IsEstado);

public sealed record ConciliacionExportDataDto(
    string NombreRegla,
    long TotalCount,
    decimal MontoAfectado,
    IReadOnlyList<ConciliacionExportColumnDto> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows);
