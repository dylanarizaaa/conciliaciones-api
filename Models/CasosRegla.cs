using System.Text.Json.Nodes;

namespace Conciliaciones.API.Models;

/// <summary>Orden realmente aplicado: { "id": "fechaTransaccion", "desc": true }.</summary>
public class OrdenAplicado
{
    public string Id { get; set; } = string.Empty;
    public bool Desc { get; set; }
}

/// <summary>Resumen superior de la pantalla: casos y monto afectado.</summary>
public class ResumenCasos
{
    public int CasosTotales { get; set; }
    public decimal MontoAfectado { get; set; }
}

/// <summary>Página de casos de una regla (contenido de "resultado").</summary>
public class PaginaCasos
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRows { get; set; }
    public int TotalPages { get; set; }
    public List<OrdenAplicado> Sort { get; set; } = [];
    public ResumenCasos Summary { get; set; } = new();
    public List<JsonObject> Rows { get; set; } = [];
}

/// <summary>Historial de un caso: misma forma que cualquier tabla, sin resumen y sin paginación real.</summary>
public class PaginaHistorial
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRows { get; set; }
    public int TotalPages { get; set; }
    public List<OrdenAplicado> Sort { get; set; } = [];
    public List<JsonObject> Rows { get; set; } = [];
}

/// <summary>Cuerpo de POST …/casos/resolver.</summary>
public class ResolverCasosRequest
{
    public List<string> Ids { get; set; } = [];
}

/// <summary>Resultado de resolver: alimenta el modal "Conciliación procesada".</summary>
public class ResolverCasosResultado
{
    public int Procesados { get; set; }
    public int Pendientes { get; set; }
    public string ValidadoPor { get; set; } = string.Empty;
    public DateTimeOffset FechaValidacion { get; set; }
}
