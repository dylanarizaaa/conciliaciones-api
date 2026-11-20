using Conciliaciones.Infraestructura.Persistence.Documents;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Projections
{
    [BsonIgnoreExtraElements]
    public sealed class ParametrizacionRulesParamsProjection
    {
        [BsonElement("parametrizacionId")]
        public string ParametrizacionId { get; set; } = string.Empty;

        [BsonElement("parametros")]
        public List<ParametroDocument>? Parametros { get; set; }

        [BsonElement("reglas")]
        public List<ReglaItemDocument>? Reglas { get; set; }
    }
}
