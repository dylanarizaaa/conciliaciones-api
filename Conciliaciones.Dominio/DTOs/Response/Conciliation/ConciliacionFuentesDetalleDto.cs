namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record DatoDetalleDto(
        string Nombre,
        string Tipo,
        object? Valor
    );

public record FuenteDetalleDto(
    string Nombre,
    List<DatoDetalleDto> Datos
);
