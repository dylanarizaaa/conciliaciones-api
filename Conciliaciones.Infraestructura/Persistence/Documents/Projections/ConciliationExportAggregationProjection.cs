using Conciliaciones.Infraestructura.Persistence.Documents;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Projections
{
    /// <summary>
    /// Resultado del aggregate de export (UI + todas las conciliaciones filtradas).
    /// DEBUG: UiConfigDoc.Fuente = columnas; ConciliacionesFiltradas.Count debe = TotalCount.
    /// </summary>
    [BsonIgnoreExtraElements]
    public sealed class ConciliationExportAggregationProjection
    {
        [BsonElement("uiConfigDoc")]
        public ParametrizacionUiDocument? UiConfigDoc { get; set; }

        [BsonElement("reglaSeleccionada")]
        public ReglaItemDocument? ReglaSeleccionada { get; set; }

        [BsonElement("totalCount")]
        public long TotalCount { get; set; }

        [BsonElement("conciliacionesFiltradas")]
        public List<ConciliacionDocument>? ConciliacionesFiltradas { get; set; }
    }
}
