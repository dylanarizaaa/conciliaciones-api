using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class SubProcesoDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.SubProceso.IdSubProceso)]
        public string IdSubProceso { get; set; } = string.Empty;

        [BsonElement(DocumentFields.SubProceso.IdProceso)]
        public string IdProceso { get; set; } = string.Empty;

        [BsonElement(DocumentFields.SubProceso.Nombre)]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement(DocumentFields.SubProceso.FechaCreacion)]
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
