using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class AutomatizacionDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement("automatizacionId")]
        public string AutomatizacionId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Parametrizacion.ParametrizacionId)]
        public string ParametrizacionId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Common.Estado)]
        public string Estado { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Common.FechaCreacion)]
        [BsonIgnoreIfNull]
        public string? FechaCreacion { get; set; }

        [BsonElement("fechaFinal")]
        [BsonIgnoreIfNull]
        public string? FechaFinal { get; set; }

        [BsonElement("reintento")]
        public int Reintento { get; set; }

        [BsonElement("intentos")]
        public int Intentos { get; set; }

        [BsonElement("esperaReintento")]
        public int EsperaReintento { get; set; }

        [BsonElement("preFuentes")]
        [BsonIgnoreIfNull]
        public List<FuenteParametrizacionDocument>? PreFuentes { get; set; }

        [BsonElement("marcaAgua")]
        [BsonIgnoreIfNull]
        public MarcaAguaDocument? MarcaAgua { get; set; }

        [BsonElement("intervalo")]
        [BsonIgnoreIfNull]
        public IntervaloDocument? Intervalo { get; set; }
    }

    public sealed class MarcaAguaDocument
    {
        [BsonElement("indiceFinal")]
        [BsonIgnoreIfNull]
        public int? IndiceFinal { get; set; }

        [BsonElement("fechaBusqueda")]
        [BsonIgnoreIfNull]
        public string? FechaBusqueda { get; set; }
    }

    public sealed class IntervaloDocument
    {
        [BsonElement("valor")]
        public int Valor { get; set; }

        [BsonElement("unidad")]
        [BsonIgnoreIfNull]
        public string? Unidad { get; set; }
    }
}
