using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class HistorialEventos
    {
        [BsonElement(DocumentFields.HistorialEventos.EventoId)]
        public string? EventoId { get; set; }
        [BsonElement(DocumentFields.HistorialEventos.ParametrizacionId)]
        public string? ParametrizacionId { get; set; }
        [BsonElement(DocumentFields.HistorialEventos.ConciliacionId)]
        public string? ConciliacionId { get; set; }
    }
}
