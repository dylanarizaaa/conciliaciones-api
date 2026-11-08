using System.Text.Json.Serialization;

namespace Conciliaciones.API.Models;

public class SubProcesoDto
{
    [JsonIgnore] public Guid ProcesoId { get; set; }
    public Guid SubProcesoId { get; set; }
    public string NombreSubProceso { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int Cantidad { get; set; }
}
