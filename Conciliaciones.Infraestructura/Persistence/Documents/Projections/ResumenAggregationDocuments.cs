using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Projections
{
    [BsonIgnoreExtraElements]
    public sealed class ProyectoResumenProjection
    {
        [BsonElement("idProyecto")]
        public string IdProyecto { get; set; } = string.Empty;

        [BsonElement("nombreProyecto")]
        public string NombreProyecto { get; set; } = string.Empty;

        [BsonElement("conciliacionesFallidas")]
        public List<ConciliacionMontoProjection>? ConciliacionesFallidas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ProcesoResumenProjection
    {
        [BsonElement("idProceso")]
        public string IdProceso { get; set; } = string.Empty;

        [BsonElement("nombreProceso")]
        public string NombreProceso { get; set; } = string.Empty;

        [BsonElement("conciliacionesFallidas")]
        public List<ConciliacionMontoProjection>? ConciliacionesFallidas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class SubProcesoResumenProjection
    {
        [BsonElement("idSubProceso")]
        public string IdSubProceso { get; set; } = string.Empty;

        [BsonElement("nombreSubProceso")]
        public string NombreSubProceso { get; set; } = string.Empty;

        [BsonElement("conciliacionesFallidas")]
        public List<ConciliacionMontoProjection>? ConciliacionesFallidas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ConciliacionMontoProjection
    {
        [BsonElement("resultadoTransformacion")]
        public Dictionary<string, object>? ResultadoTransformacion { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ReglaFallidaProjection
    {
        [BsonElement("idRegla")]
        public string? IdRegla { get; set; }

        [BsonElement("nombreRegla")]
        public string? NombreRegla { get; set; }

        [BsonElement("casos")]
        public long Casos { get; set; }

        [BsonElement("conciliacionesFiltradas")]
        public List<ConciliacionMontoProjection>? ConciliacionesFiltradas { get; set; }
    }

    [BsonIgnoreExtraElements]
    public sealed class ReglasFallidasTotalesProjection
    {
        [BsonElement("casosTotales")]
        public long CasosTotales { get; set; }
    }
}
