using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services
{
    public sealed class ConciliationRowsComposer : IConciliationRowsComposer
    {
        private readonly ITransformacionMontoCalculator _montoCalculator;
        private readonly IUiConfigColumnExtractor _columnExtractor;

        public ConciliationRowsComposer(
            ITransformacionMontoCalculator montoCalculator,
            IUiConfigColumnExtractor columnExtractor)
        {
            _montoCalculator = montoCalculator ?? throw new ArgumentNullException(nameof(montoCalculator));
            _columnExtractor = columnExtractor ?? throw new ArgumentNullException(nameof(columnExtractor));
        }

        public PagedRowsDto Compose(ConciliationRowsAggregationProjection? groupResult, int pageNumber, int pageSize)
        {
            if (groupResult == null)
            {
                return CreatePaged(0, 0m, 0, pageNumber, pageSize, new List<Dictionary<string, object?>>());
            }

            List<ConciliacionDocument> conciliaciones = groupResult.ConciliacionesPaginadas ?? new List<ConciliacionDocument>();
            List<(string FuenteId, string PropertyName)> mappings =
                _columnExtractor.ExtractVisibleFieldMappings(groupResult.UiConfigDoc);
            List<Dictionary<string, object?>> rows = conciliaciones.Select(c => MapRow(c, mappings)).ToList();

            decimal montoTotal = TransformacionMontoReader.SumFromConciliaciones(
                conciliaciones.Select(c => c.ResultadoTransformacion),
                _montoCalculator);

            return CreatePaged(conciliaciones.Count, montoTotal, groupResult.TotalCount, pageNumber, pageSize, rows);
        }

        private static Dictionary<string, object?> MapRow(
            ConciliacionDocument concDoc,
            List<(string FuenteId, string PropertyName)> fieldMappings)
        {
            Dictionary<string, object?> row = new Dictionary<string, object?>
            {
                ["conciliacionId"] = concDoc.ConciliacionId ?? string.Empty
            };

            Dictionary<string, Dictionary<string, object>> fuentesDict = BuildFuentesDict(concDoc);
            foreach ((string FuenteId, string PropertyName) mapping in fieldMappings)
            {
                if (IsSystemField(mapping.PropertyName))
                {
                    continue;
                }

                row[mapping.PropertyName] = ResolveFieldValue(fuentesDict, mapping);
            }

            row["estadoConciliacion"] = concDoc.Estado ?? string.Empty;
            return row;
        }

        private static Dictionary<string, Dictionary<string, object>> BuildFuentesDict(ConciliacionDocument concDoc)
        {
            Dictionary<string, Dictionary<string, object>> fuentesDict = new Dictionary<string, Dictionary<string, object>>();
            if (concDoc.ResultadoFuentes == null)
            {
                return fuentesDict;
            }

            foreach (ResultadoFuenteDocument fDoc in concDoc.ResultadoFuentes)
            {
                if (fDoc.Data != null && fDoc.Data.Count > 0)
                {
                    fuentesDict[fDoc.FuenteId ?? string.Empty] = fDoc.Data[0];
                }
            }

            return fuentesDict;
        }

        private static object? ResolveFieldValue(
            Dictionary<string, Dictionary<string, object>> fuentesDict,
            (string FuenteId, string PropertyName) mapping)
        {
            if (string.IsNullOrEmpty(mapping.FuenteId)
                || !fuentesDict.TryGetValue(mapping.FuenteId, out Dictionary<string, object>? dataDoc)
                || !dataDoc.TryGetValue(mapping.PropertyName, out object? raw)
                || raw == null)
            {
                return null;
            }

            return raw;
        }

        private static bool IsSystemField(string propertyName) =>
            propertyName.Equals("estado", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("accion", StringComparison.OrdinalIgnoreCase);

        private static PagedRowsDto CreatePaged(
            long casosTotales,
            decimal montoAfectadoTotal,
            long totalCount,
            int pageNumber,
            int pageSize,
            List<Dictionary<string, object?>> rows)
        {
            long totalPages = pageSize > 0 ? (long)Math.Ceiling((double)totalCount / pageSize) : 0;
            return new PagedRowsDto(casosTotales, montoAfectadoTotal, totalCount, totalPages, pageNumber, pageSize, rows);
        }
    }
}
