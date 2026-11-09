namespace Conciliaciones.API.Models;

/// <summary>Envoltorio estándar de todas las respuestas del microservicio.</summary>
public class RespuestaApi<T>
{
    public int CodigoEstado { get; set; }
    public bool RespuestaEstado { get; set; }
    public string? Mensaje { get; set; }
    public List<string> Errores { get; set; } = [];
    public string? MensajeError { get; set; }
    public T? Resultado { get; set; }

    public static RespuestaApi<T> Ok(T resultado, string? mensaje = null) => new()
    {
        CodigoEstado = StatusCodes.Status200OK,
        RespuestaEstado = true,
        Mensaje = mensaje,
        Resultado = resultado
    };

    public static RespuestaApi<T> Error(int codigoEstado, string mensajeError, params string[] errores) => new()
    {
        CodigoEstado = codigoEstado,
        RespuestaEstado = false,
        MensajeError = mensajeError,
        Errores = [.. errores]
    };
}

/// <summary>Contenedor de listas: { "data": [ ... ] }.</summary>
public class ResultadoLista<T>
{
    public List<T> Data { get; set; } = [];
}

/// <summary>Lista con totales: { "totalAfectado", "casosTotales", "data": [ ... ] }.</summary>
public class ResultadoListaConTotales<T>
{
    public decimal TotalAfectado { get; set; }
    public int CasosTotales { get; set; }
    public List<T> Data { get; set; } = [];
}

/// <summary>Contenedor de un solo objeto: { "data": { ... } }.</summary>
public class ResultadoDato<T>
{
    public T? Data { get; set; }
}

/// <summary>Página de resultados con totales de casos y monto.</summary>
public class ResultadoPaginado<T>
{
    public int CasosTotales { get; set; }
    public decimal MontoAfectado { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = [];
}
