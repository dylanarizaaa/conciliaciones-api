namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

/// <summary>
/// Dato de una fuente en <c>fuentes-detalle</c>. <paramref name="Id"/> es el <c>accessorKey</c> de la
/// columna en la plantilla del acordeón: el front une el dato con su columna por ese id.
/// </summary>
public record DatoDetalleDto(
        string Id,
        string Nombre,
        string Tipo,
        object? Valor
    );

/// <summary>
/// Fuente de <c>fuentes-detalle</c>. <paramref name="Id"/> es el <c>idFuente</c>, que la plantilla usa
/// como <c>accordion.items[].id</c>: el front une la fuente con su acordeón por ese id.
/// </summary>
public record FuenteDetalleDto(
    string Id,
    string Nombre,
    List<DatoDetalleDto> Datos
);
