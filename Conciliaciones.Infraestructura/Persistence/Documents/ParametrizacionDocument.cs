using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Conciliaciones.Infraestructura.Persistence.Documents
{
    public sealed class ParametrizacionDocument
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }

        [BsonElement(DocumentFields.Parametrizacion.ParametrizacionId)]
        public string ParametrizacionId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Parametrizacion.SubProcesoId)]
        public string SubProcesoId { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Parametrizacion.FechaCreacion)]
        public DateTime? FechaCreacion { get; set; }

        [BsonElement(DocumentFields.Parametrizacion.Parametros)]
        [BsonIgnoreIfNull]
        public List<ParametroDocument>? Parametros { get; set; }

        [BsonElement(DocumentFields.Parametrizacion.Fuentes)]
        [BsonIgnoreIfNull]
        public List<FuenteParametrizacionDocument>? Fuentes { get; set; }

        [BsonElement(DocumentFields.Parametrizacion.Transformacion)]
        [BsonIgnoreIfNull]
        public List<TransformacionDocument>? Transformacion { get; set; }

        [BsonElement(DocumentFields.Parametrizacion.Reglas)]
        [BsonIgnoreIfNull]
        public List<ReglaItemDocument>? Reglas { get; set; }
    }

    public sealed class ParametroDocument
    {
        [BsonElement("nombre")]
        [BsonIgnoreIfNull]
        public string? Nombre { get; set; }

        [BsonElement("tipo")]
        [BsonIgnoreIfNull]
        public string? Tipo { get; set; }
    }

    public sealed class FuenteParametrizacionDocument
    {
        [BsonElement(DocumentFields.Conciliacion.FuenteId)]
        [BsonIgnoreIfNull]
        public string? FuenteId { get; set; }

        [BsonElement(DocumentFields.Conciliacion.ConexionId)]
        [BsonIgnoreIfNull]
        public string? ConexionId { get; set; }

        [BsonElement("tabla")]
        [BsonIgnoreIfNull]
        public string? Tabla { get; set; }

        [BsonElement("camposSeleccion")]
        [BsonIgnoreIfNull]
        public List<CampoSeleccionDocument>? CamposSeleccion { get; set; }

        [BsonElement("filtros")]
        [BsonIgnoreIfNull]
        public List<FiltroDocument>? Filtros { get; set; }
    }

    public sealed class CampoSeleccionDocument
    {
        [BsonElement("nombre")]
        [BsonIgnoreIfNull]
        public string? Nombre { get; set; }

        [BsonElement("tipoDato")]
        [BsonIgnoreIfNull]
        public string? TipoDato { get; set; }

        [BsonElement("alias")]
        [BsonIgnoreIfNull]
        public string? Alias { get; set; }
    }

    public sealed class FiltroDocument
    {
        [BsonElement("operador")]
        [BsonIgnoreIfNull]
        public string? Operador { get; set; }

        [BsonElement("condiciones")]
        [BsonIgnoreIfNull]
        public List<CondicionFiltroDocument>? Condiciones { get; set; }
    }

    public sealed class CondicionFiltroDocument
    {
        [BsonElement("campo")]
        [BsonIgnoreIfNull]
        public string? Campo { get; set; }

        [BsonElement("operador")]
        [BsonIgnoreIfNull]
        public string? Operador { get; set; }

        [BsonElement("valor")]
        [BsonIgnoreIfNull]
        public BsonValue? Valor { get; set; }
    }

    public sealed class TransformacionDocument
    {
        [BsonElement("alias")]
        [BsonIgnoreIfNull]
        public string? Alias { get; set; }

        [BsonElement("tipo")]
        [BsonIgnoreIfNull]
        public string? Tipo { get; set; }

        [BsonElement("operacion")]
        [BsonIgnoreIfNull]
        public string? Operacion { get; set; }

        [BsonElement("operandos")]
        [BsonIgnoreIfNull]
        public List<OperandoDocument>? Operandos { get; set; }
    }

    public sealed class OperandoDocument
    {
        [BsonElement("ref")]
        [BsonIgnoreIfNull]
        public string? Ref { get; set; }
    }

    public sealed class ReglaItemDocument
    {
        [BsonElement("idRegla")]
        public string IdRegla { get; set; } = string.Empty;

        [BsonElement(DocumentFields.Conciliacion.NombreRegla)]
        public string NombreRegla { get; set; } = string.Empty;

        [BsonElement("eventoExito")]
        public string EventoExito { get; set; } = string.Empty;

        [BsonElement("mensajeError")]
        public string MensajeError { get; set; } = string.Empty;

        [BsonElement("expresion")]
        public string Expresion { get; set; } = string.Empty;
    }
}
