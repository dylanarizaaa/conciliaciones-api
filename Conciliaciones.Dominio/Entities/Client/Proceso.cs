using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Client
{
    public sealed class Proceso
    {
        private Proceso(string idProceso, string idProyecto, string nombre, DateTime fechaCreacion)
        {
            IdProceso = idProceso;
            IdProyecto = idProyecto;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
        }

        public string IdProceso { get; }

        public string IdProyecto { get; }

        public string Nombre { get; }

        public DateTime FechaCreacion { get; }

        public static Proceso Create(string nombre, string idProyecto, string? idProceso = null)
        {
            string normalizedName = EntityValidation.Required(nombre, nameof(nombre));
            string parentId = EntityValidation.Required(idProyecto, nameof(idProyecto));
            string id = string.IsNullOrWhiteSpace(idProceso)
                ? EntityValidation.NewId()
                : idProceso.Trim();

            return new Proceso(id, parentId, normalizedName, DateTime.UtcNow);
        }

        public static Proceso Rehydrate(string idProceso, string idProyecto, string nombre, DateTime fechaCreacion) =>
            new Proceso(
                EntityValidation.Required(idProceso, nameof(idProceso)),
                EntityValidation.Required(idProyecto, nameof(idProyecto)),
                EntityValidation.Required(nombre, nameof(nombre)),
                fechaCreacion);
    }

    [Obsolete("Usar Proceso.")]
    public class Process
    {
        public string? Id { get; set; }
        public string? IdProyecto { get; set; }
        public string? IdProceso { get; set; }
        public string? Nombre { get; set; }
    }
}
