using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    [BsonIgnoreExtraElements]
    public sealed class EventoDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement("eventoId")]
        public string EventoId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Parametrizacion.ParametrizacionId)]
        [BsonIgnoreIfNull]
        public string? ParametrizacionId { get; set; }

        [BsonElement(DocumentFields.Common.FechaCreacion)]
        public DateTime FechaCreacion { get; set; }

        [BsonElement("fechaProcesamiento")]
        [BsonIgnoreIfNull]
        public DateTime? FechaProcesamiento { get; set; }

        [BsonElement("parametros")]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? Parametros { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ConciliacionId)]
        public string ConciliacionId { get; set; } = string.Empty;
    }
}
