using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class HistorialConciliacionesDocument : ConciliacionDocument
    {
        [BsonElement(DocumentFields.HistorialConciliacion.EventoId)]
        public string? EventoId { get; set; }

        [BsonElement(DocumentFields.HistorialConciliacion.Auditoria)]
        public Auditoria? Auditoria { get; set; }
    }


    public sealed class Auditoria
    {

        [BsonElement(DocumentFields.Auditoria.UsuarioProceso)]
        public string? UsuarioProceso { get; set; }

        [BsonElement(DocumentFields.Auditoria.FechaHistorial)]
        public DateTime? FechaHistorial { get; set; }

        [BsonElement(DocumentFields.Auditoria.Origen)]
        public string? Origen { get; set; }

    }
}
