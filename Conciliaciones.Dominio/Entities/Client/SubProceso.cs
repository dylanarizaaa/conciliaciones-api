using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Client
{
    public sealed class SubProceso
    {
        private SubProceso(string idSubProceso, string idProceso, string nombre, DateTime fechaCreacion)
        {
            IdSubProceso = idSubProceso;
            IdProceso = idProceso;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
        }

        public string IdSubProceso { get; }

        public string IdProceso { get; }

        public string Nombre { get; }

        public DateTime FechaCreacion { get; }

        public static SubProceso Create(string nombre, string idProceso, string? idSubProceso = null)
        {
            string normalizedName = EntityValidation.Required(nombre, nameof(nombre));
            string parentId = EntityValidation.Required(idProceso, nameof(idProceso));
            string id = string.IsNullOrWhiteSpace(idSubProceso)
                ? EntityValidation.NewId()
                : idSubProceso.Trim();

            return new SubProceso(id, parentId, normalizedName, DateTime.UtcNow);
        }

        public static SubProceso Rehydrate(string idSubProceso, string idProceso, string nombre, DateTime fechaCreacion) =>
            new SubProceso(
                EntityValidation.Required(idSubProceso, nameof(idSubProceso)),
                EntityValidation.Required(idProceso, nameof(idProceso)),
                EntityValidation.Required(nombre, nameof(nombre)),
                fechaCreacion);
    }

    [Obsolete("Usar SubProceso.")]
    public class SubProcess
    {
        public string? Id { get; set; }
        public string? IdProceso { get; set; }
        public string? IdSubproceso { get; set; }
        public string? Nombre { get; set; }
    }
}
