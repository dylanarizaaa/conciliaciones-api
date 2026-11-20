using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    /// <summary>
    /// Documento Mongo de conciliaciones. Extra fields (p. ej. historial) se ignoran a propósito.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ConciliacionDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ConciliacionId)]
        public string ConciliacionId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Conciliacion.ParametrizacionId)]
        [BsonIgnoreIfNull]
        public string? ParametrizacionId { get; set; }

        [BsonElement(DocumentFields.Conciliacion.Estado)]
        [BsonIgnoreIfNull]
        public string? Estado { get; set; }

        [BsonElement(DocumentFields.Conciliacion.FechaCreacion)]
        [BsonIgnoreIfNull]
        public DateTime? FechaCreacion { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ParametrosEntrada)]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? ParametrosEntrada { get; set; }

        /// <summary>Bag dinámico: claves variables por transformación (compatibilidad Mongo).</summary>
        [BsonElement(DocumentFields.Conciliacion.ResultadoTransformacion)]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? ResultadoTransformacion { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ResultadoFuentes)]
        [BsonIgnoreIfNull]
        public List<ResultadoFuenteDocument>? ResultadoFuentes { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ResultadoRegla)]
        [BsonIgnoreIfNull]
        public ResultadoReglaDocument? ResultadoRegla { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ResultadoFuenteDocument
    {
        [BsonElement(DocumentFields.Conciliacion.FuenteId)]
        [BsonIgnoreIfNull]
        public string? FuenteId { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ConexionId)]
        [BsonIgnoreIfNull]
        public string? ConexionId { get; set; }

        [BsonElement(DocumentFields.Conciliacion.Data)]
        [BsonIgnoreIfNull]
        public List<Dictionary<string, object>>? Data { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ResultadoReglaDocument
    {
        [BsonElement(DocumentFields.Conciliacion.NombreRegla)]
        [BsonIgnoreIfNull]
        public string? NombreRegla { get; set; }

        [BsonElement("cumplida")]
        [BsonIgnoreIfNull]
        public bool? Cumplida { get; set; }

        [BsonElement("eventoExito")]
        [BsonIgnoreIfNull]
        public string? EventoExito { get; set; }

        [BsonElement("mensajeError")]
        [BsonIgnoreIfNull]
        public string? MensajeError { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ConciliacionIdProjection
    {
        [BsonElement(DocumentFields.Conciliacion.ConciliacionId)]
        public string ConciliacionId { get; set; } = string.Empty;
    }
}
