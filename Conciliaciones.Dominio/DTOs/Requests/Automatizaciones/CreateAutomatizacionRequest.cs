namespace Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;

public sealed class CreateAutomatizacionRequest
{
    public string? ParametrizacionId { get; set; }
    public string? Estado { get; set; }
    public string? FechaFinal { get; set; }
    public int Reintento { get; set; }
    public int Intentos { get; set; }
    public int EsperaReintento { get; set; }
    public List<AutomatizacionFuenteDto>? PreFuentes { get; set; }
    public MarcaAguaDto? MarcaAgua { get; set; }
    public IntervaloDto? Intervalo { get; set; }
}

public sealed class AutomatizacionFuenteDto
{
    public string? FuenteId { get; set; }
    public string? ConexionId { get; set; }
    public string? Tabla { get; set; }
    public List<AutomatizacionCampoSeleccionDto>? CamposSeleccion { get; set; }
    public List<AutomatizacionFiltroDto>? Filtros { get; set; }
}

public sealed class AutomatizacionCampoSeleccionDto
{
    public string? Nombre { get; set; }
    public string? TipoDato { get; set; }
    public string? Alias { get; set; }
}

public sealed class AutomatizacionFiltroDto
{
    public string? Operador { get; set; }
    public List<AutomatizacionCondicionDto>? Condiciones { get; set; }
}

public sealed class AutomatizacionCondicionDto
{
    public string? Campo { get; set; }
    public string? Operador { get; set; }
    public string? Valor { get; set; }
}

public sealed class MarcaAguaDto
{
    public int? IndiceFinal { get; set; }
    public string? FechaBusqueda { get; set; }
}

public sealed class IntervaloDto
{
    public int Valor { get; set; }
    public string? Unidad { get; set; }
}
