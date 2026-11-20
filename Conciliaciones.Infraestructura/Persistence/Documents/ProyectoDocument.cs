using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class ProyectoDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.Proyecto.IdProyecto)]
        public string IdProyecto { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Proyecto.Nombre)]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Proyecto.FechaCreacion)]
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
