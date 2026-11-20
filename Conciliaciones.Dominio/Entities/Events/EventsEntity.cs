namespace Conciliaciones.Dominio.Entities.Events
{
    public class EventsEntity
    {
        public string? Id { get; set; }

        public string? EventoId { get; set; }

        public string? ParametrizacionId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public IDictionary<string, object>? Parametros { get; set; }

        public string? ConciliacionId { get; set; }

        public bool Reintento { get; set; }
    }
}
