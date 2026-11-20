using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Conciliation
{
    public sealed class ConciliacionEntity
    {
        private ConciliacionEntity(
            string conciliacionId,
            string? parametrizacionId,
            string? estado,
            IReadOnlyDictionary<string, object>? resultadoTransformacion,
            IReadOnlyList<ResultadoFuenteEntity>? resultadoFuentes,
            ResultadoReglaEntity? resultadoRegla)
        {
            ConciliacionId = conciliacionId;
            ParametrizacionId = parametrizacionId;
            Estado = estado;
            ResultadoTransformacion = resultadoTransformacion;
            ResultadoFuentes = resultadoFuentes;
            ResultadoRegla = resultadoRegla;
        }

        public string ConciliacionId { get; }

        public string? ParametrizacionId { get; }

        public string? Estado { get; }

        /// <summary>
        /// Bag dinámico compatible con Mongo (claves de transformación variables por parametrización).
        /// </summary>
        public IReadOnlyDictionary<string, object>? ResultadoTransformacion { get; }

        public IReadOnlyList<ResultadoFuenteEntity>? ResultadoFuentes { get; }

        public ResultadoReglaEntity? ResultadoRegla { get; }

        public bool EsFallidaOError =>
            !string.IsNullOrWhiteSpace(Estado)
            && ConciliacionEstados.FallidosOError.Contains(Estado, StringComparer.OrdinalIgnoreCase);

        public static ConciliacionEntity Rehydrate(
            string conciliacionId,
            string? parametrizacionId,
            string? estado,
            IReadOnlyDictionary<string, object>? resultadoTransformacion,
            IEnumerable<ResultadoFuenteEntity>? resultadoFuentes,
            ResultadoReglaEntity? resultadoRegla) =>
            new ConciliacionEntity(
                EntityValidation.Required(conciliacionId, nameof(conciliacionId)),
                parametrizacionId,
                estado,
                resultadoTransformacion,
                resultadoFuentes?.ToList(),
                resultadoRegla);
    }

    public sealed class ResultadoFuenteEntity
    {
        public ResultadoFuenteEntity(
            string? fuenteId,
            string? conexionId,
            IReadOnlyList<IReadOnlyDictionary<string, object>>? data)
        {
            FuenteId = EntityValidation.OptionalTrim(fuenteId);
            ConexionId = EntityValidation.OptionalTrim(conexionId);
            Data = data;
        }

        public string FuenteId { get; }

        public string ConexionId { get; }

        public IReadOnlyList<IReadOnlyDictionary<string, object>>? Data { get; }
    }

    public sealed class ResultadoReglaEntity
    {
        public ResultadoReglaEntity(string? nombreRegla)
        {
            NombreRegla = EntityValidation.OptionalTrim(nombreRegla);
        }

        public string NombreRegla { get; }
    }
}
