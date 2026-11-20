namespace Conciliaciones.Dominio.DTOs.Response.SubProcess;

public record FailedRuleDto
(
string IdRegla,
string nombreRegla,
long Casos,
decimal Monto
);

public record SubprocesoReglasResumenDto(
    long CasosTotales,
    decimal MontoAfectado,
    int PageNumber,
    int PageSize,
    long TotalPages,
    List<FailedRuleDto> Data
);