using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class ProcesoDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.Proceso.IdProceso)]
        public string IdProceso { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Proceso.IdProyecto)]
        public string IdProyecto { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Proceso.Nombre)]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Proceso.FechaCreacion)]
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
