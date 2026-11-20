using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class ParametrizacionUiDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.IdParametrizacion)]
        public string IdParametrizacion { get; set; } = string.Empty;

        [BsonElement(DocumentFields.ParametrizacionUi.Fuente)]
        [BsonIgnoreIfNull]
        public List<UiColumnDocument>? Fuente { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.Datos)]
        [BsonIgnoreIfNull]
        public List<UiDatosBlockDocument>? Datos { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.Blocks)]
        [BsonIgnoreIfNull]
        public List<UiLegacyBlockDocument>? Blocks { get; set; }

        /// <summary>Plantilla de front: claves table y/o accordion. No reemplaza fuente/datos.</summary>
        [BsonElement("plantilla")]
        [BsonIgnoreIfNull]
        public BsonDocument? Plantilla { get; set; }
    }

    public sealed class UiColumnDocument
    {
        [BsonElement(DocumentFields.ParametrizacionUi.Field)]
        public string Field { get; set; } = string.Empty;

        [BsonElement(DocumentFields.ParametrizacionUi.Header)]
        public string Header { get; set; } = string.Empty;

        [BsonElement(DocumentFields.ParametrizacionUi.Sortable)]
        public bool Sortable { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.Visible)]
        public bool Visible { get; set; } = true;

        [BsonElement(DocumentFields.ParametrizacionUi.Order)]
        public int Order { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.CellConfig)]
        [BsonIgnoreIfNull]
        public UiCellConfigDocument? CellConfig { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.Cell)]
        [BsonIgnoreIfNull]
        public UiCellConfigDocument? Cell { get; set; }
    }

    public sealed class UiCellConfigDocument
    {
        [BsonElement(DocumentFields.ParametrizacionUi.Type)]
        public string Type { get; set; } = "text";

        [BsonElement(DocumentFields.ParametrizacionUi.Format)]
        [BsonIgnoreIfNull]
        public string? Format { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.Config)]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? Config { get; set; }

        [BsonElement(DocumentFields.ParametrizacionUi.VariantByValue)]
        [BsonIgnoreIfNull]
        public Dictionary<string, string>? VariantByValue { get; set; }
    }

    public sealed class UiDatosBlockDocument
    {
        [BsonElement(DocumentFields.ParametrizacionUi.IdFuente)]
        public string IdFuente { get; set; } = string.Empty;

        [BsonElement(DocumentFields.ParametrizacionUi.Columns)]
        public List<UiColumnDocument> Columns { get; set; } = new();
    }

    public sealed class UiLegacyBlockDocument
    {
        [BsonElement(DocumentFields.ParametrizacionUi.Config)]
        [BsonIgnoreIfNull]
        public UiLegacyBlockConfigDocument? Config { get; set; }
    }

    public sealed class UiLegacyBlockConfigDocument
    {
        [BsonElement(DocumentFields.ParametrizacionUi.Columns)]
        [BsonIgnoreIfNull]
        public List<UiColumnDocument>? Columns { get; set; }
    }
}
