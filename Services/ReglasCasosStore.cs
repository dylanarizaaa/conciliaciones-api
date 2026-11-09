using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Conciliaciones.API.Models;

namespace Conciliaciones.API.Services;

/// <summary>Error de validación de parámetros (se traduce a 400).</summary>
public sealed class ParametroInvalidoException(string mensaje) : Exception(mensaje);

/// <summary>
/// Plantillas y casos de ejemplo en memoria para la pantalla Reglas.
/// La plantilla cambia por regla: cada regla registrada tiene la suya.
/// </summary>
public static class ReglasCasosStore
{
    private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "Data");

    private static readonly Lazy<JsonObject> PlantillaBase = new(() => Cargar("regla-diferencias.plantilla.json"));
    private static readonly Lazy<JsonObject> DatosBase = new(() => Cargar("regla-diferencias.datos.json"));

    // Título de la plantilla por regla (clave: nombre de la regla, sin distinguir mayúsculas).
    // Para dar a una regla columnas distintas, basta con reemplazar su entrada por su propia plantilla.
    private static readonly Dictionary<string, string> TitulosPorRegla = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ValidacionMontoExacto"] = "Diferencias de monto exacto",
        ["Cambio de estado logístico"] = "Diferencias de la regla"
    };

    private static JsonObject Cargar(string archivo) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(DataDir, archivo)))!.AsObject();

    public static bool ExisteRegla(string regla) => TitulosPorRegla.ContainsKey(regla);

    /// <summary>Plantilla de la regla (copia: el original no se modifica).</summary>
    public static JsonObject ObtenerPlantilla(string regla)
    {
        var plantilla = PlantillaBase.Value.DeepClone().AsObject();
        plantilla["title"] = TitulosPorRegla[regla];
        return plantilla;
    }

    private sealed record Columna(string Id, string Label, string Ruta, string SortType, bool Ordenable);

    private static List<Columna> ColumnasDe(string regla)
    {
        var columnas = ObtenerPlantilla(regla)["blocks"]![0]!["table"]!["columns"]!.AsArray();
        return columnas
            .Where(c => c!["accessorKey"] is not null)
            .Select(c => new Columna(
                c!["id"]!.GetValue<string>(),
                c["header"]?["inputs"]?["label"]?.GetValue<string>() ?? c["id"]!.GetValue<string>(),
                c["accessorKey"]!.GetValue<string>(),
                c["sortType"]?.GetValue<string>() ?? "text",
                c["enableSorting"]?.GetValue<bool>() ?? false))
            .ToList();
    }

    private static JsonNode? Valor(JsonObject fila, string ruta)
    {
        JsonNode? actual = fila;
        foreach (var parte in ruta.Split('.'))
        {
            actual = actual?[parte];
            if (actual is null) return null;
        }
        return actual;
    }

    private static List<(Columna Columna, bool Desc)> ParsearOrden(string regla, IEnumerable<string> sort)
    {
        var pedidos = sort.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (pedidos.Count > 1)
            throw new ParametroInvalidoException("El orden admite una sola columna (sort=campo:asc|desc).");

        var columnas = ColumnasDe(regla);
        var resultado = new List<(Columna, bool)>();
        foreach (var pedido in pedidos)
        {
            var partes = pedido.Split(':');
            if (partes.Length != 2 || partes[1] is not ("asc" or "desc"))
                throw new ParametroInvalidoException($"Formato de orden inválido: '{pedido}'. Use campo:asc o campo:desc.");

            var columna = columnas.FirstOrDefault(c => c.Id == partes[0]);
            if (columna is null || !columna.Ordenable)
                throw new ParametroInvalidoException($"La columna '{partes[0]}' no admite ordenamiento.");

            resultado.Add((columna, partes[1] == "desc"));
        }
        return resultado;
    }

    private static List<JsonObject> Ordenar(List<JsonObject> filas, List<(Columna Columna, bool Desc)> orden)
    {
        if (orden.Count == 0) return filas;
        var (columna, desc) = orden[0];

        IComparable? Clave(JsonObject fila)
        {
            var nodo = Valor(fila, columna.Ruta);
            if (nodo is null) return null;
            return columna.SortType switch
            {
                "basic" => nodo.GetValue<decimal>(),
                "datetime" => DateTimeOffset.Parse(nodo.GetValue<string>(), CultureInfo.InvariantCulture),
                _ => nodo.GetValue<string>().ToLowerInvariant()
            };
        }

        return desc
            ? [.. filas.OrderByDescending(Clave, Comparer<IComparable?>.Default)]
            : [.. filas.OrderBy(Clave, Comparer<IComparable?>.Default)];
    }

    private static List<JsonObject> Filas() =>
        [.. DatosBase.Value["rows"]!.AsArray().Select(f => f!.DeepClone().AsObject())];

    public static PaginaCasos ObtenerPagina(string regla, int pageNumber, int pageSize, IEnumerable<string> sort)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ParametroInvalidoException("pageNumber y pageSize deben ser mayores o iguales a 1.");

        var orden = ParsearOrden(regla, sort);
        var filas = Ordenar(Filas(), orden);

        return new PaginaCasos
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRows = filas.Count,
            TotalPages = (int)Math.Ceiling(filas.Count / (double)pageSize),
            Sort = [.. orden.Select(o => new OrdenAplicado { Id = o.Columna.Id, Desc = o.Desc })],
            Summary = new ResumenCasos
            {
                CasosTotales = filas.Count,
                MontoAfectado = filas.Sum(f => f["diferencia"]!.GetValue<decimal>())
            },
            // Si pageNumber supera totalPages, rows va vacío con los totales reales.
            Rows = [.. filas.Skip((pageNumber - 1) * pageSize).Take(pageSize)]
        };
    }

    /// <summary>CSV (UTF-8 con BOM, para que Excel respete las tildes) con el orden pedido.</summary>
    public static byte[] GenerarReporteCsv(string regla, IEnumerable<string> sort)
    {
        var columnas = ColumnasDe(regla);
        var filas = Ordenar(Filas(), ParsearOrden(regla, sort));

        static string Esc(string? v) =>
            v is not null && v.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{v.Replace("\"", "\"\"")}\"" : v ?? "";

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', columnas.Select(c => Esc(c.Label))));
        foreach (var fila in filas)
        {
            sb.AppendLine(string.Join(',', columnas.Select(c =>
                Esc(Valor(fila, c.Ruta) switch
                {
                    null => null,
                    JsonValue v when v.TryGetValue<string>(out var s) => s,
                    var n => n.ToJsonString()
                }))));
        }
        return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }
}
