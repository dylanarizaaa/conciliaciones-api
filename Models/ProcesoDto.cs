using System.Text.Json.Serialization;

namespace Conciliaciones.API.Models;

public class ProcesoDto
{
    [JsonIgnore] public Guid ProyectoId { get; set; }
    public Guid ProcesoId { get; set; }
    public string NombreProceso { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int Cantidad { get; set; }
}
