namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record PagedRowsDto(
long CasosTotales,
decimal MontoAfectado,
long TotalCount,
long TotalPages,
int PageNumber,
int PageSize,
 List<Dictionary<string, object?>> Rows
);

