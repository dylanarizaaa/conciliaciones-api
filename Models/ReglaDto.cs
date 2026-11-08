using System.Text.Json.Serialization;

namespace Conciliaciones.API.Models;

public class ReglaDto
{
    [JsonIgnore] public Guid SubProcesoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Casos { get; set; }
    public decimal Monto { get; set; }
}
