using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Client
{
    public sealed class Proyecto
    {
        private Proyecto(string idProyecto, string nombre, DateTime fechaCreacion)
        {
            IdProyecto = idProyecto;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
        }

        public string IdProyecto { get; }

        public string Nombre { get; }

        public DateTime FechaCreacion { get; }

        public static Proyecto Create(string nombre, string? idProyecto = null)
        {
            string normalizedName = EntityValidation.Required(nombre, nameof(nombre));
            string id = string.IsNullOrWhiteSpace(idProyecto)
                ? EntityValidation.NewId()
                : idProyecto.Trim();

            return new Proyecto(id, normalizedName, DateTime.UtcNow);
        }

        public static Proyecto Rehydrate(string idProyecto, string nombre, DateTime fechaCreacion) =>
            new Proyecto(
                EntityValidation.Required(idProyecto, nameof(idProyecto)),
                EntityValidation.Required(nombre, nameof(nombre)),
                fechaCreacion);
    }

    /// <summary>Compatibilidad con el nombre legacy.</summary>
    [Obsolete("Usar Proyecto.")]
    public class Proyect
    {
        public string? Id { get; set; }
        public string? IdProyecto { get; set; }
        public string? Nombre { get; set; }
    }
}
