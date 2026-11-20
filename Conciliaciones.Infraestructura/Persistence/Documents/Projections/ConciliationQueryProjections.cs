using Conciliaciones.Infraestructura.Persistence.Documents;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Projections
{
    [BsonIgnoreExtraElements]
    public sealed class ConciliationUiAggregationProjection
    {
        [BsonElement("uiConfigDoc")]
        public ParametrizacionUiDocument? UiConfigDoc { get; set; }

        [BsonElement("conciliacionesFiltradas")]
        public List<ConciliacionDocument>? ConciliacionesFiltradas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ConciliationRowsAggregationProjection
    {
        [BsonElement("uiConfigDoc")]
        public ParametrizacionUiDocument? UiConfigDoc { get; set; }

        [BsonElement("totalCount")]
        public long TotalCount { get; set; }

        [BsonElement("conciliacionesPaginadas")]
        public List<ConciliacionDocument>? ConciliacionesPaginadas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ConciliationFuentesDetailProjection
    {
        [BsonElement("conciliacionId")]
        public string? ConciliacionId { get; set; }

        [BsonElement("nombreProceso")]
        public string? NombreProceso { get; set; }

        [BsonElement("resultadoFuentes")]
        public List<ResultadoFuenteDocument>? ResultadoFuentes { get; set; }

        [BsonElement("uiConfigDoc")]
        public ParametrizacionUiDocument? UiConfigDoc { get; set; }
    }
}
