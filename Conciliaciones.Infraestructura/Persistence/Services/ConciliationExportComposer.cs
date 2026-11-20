using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace Conciliaciones.Infraestructura.Persistence.Services
{
    /// <summary>
    /// Arma columnas UI + filas export.
    /// Reglas: todas las Visible de Fuente; excluye "accion"; "estado" sale de ConciliacionDocument.Estado;
    /// el resto se resuelve por Field "fuenteId.propiedad" contra ResultadoFuentes.
    /// </summary>
    public sealed class ConciliationExportComposer : IConciliationExportComposer
    {
        private readonly ITransformacionMontoCalculator _montoCalculator;
        private readonly ILogger<ConciliationExportComposer> _logger;

        public ConciliationExportComposer(
            ITransformacionMontoCalculator montoCalculator,
            ILogger<ConciliationExportComposer> logger)
        {
            _montoCalculator = montoCalculator ?? throw new ArgumentNullException(nameof(montoCalculator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ConciliacionExportDataDto Compose(
            ConciliationExportAggregationProjection? projection,
            string reglaId)
        {
            string nombreRegla = string.IsNullOrWhiteSpace(projection?.ReglaSeleccionada?.NombreRegla)
                ? reglaId
                : projection!.ReglaSeleccionada!.NombreRegla;

            if (projection == null)
            {
                _logger.LogWarning(
                    "[ExportExcel][Compose] projection null. reglaId={ReglaId}",
                    reglaId);
                return new ConciliacionExportDataDto(
                    nombreRegla,
                    0,
                    0m,
                    Array.Empty<ConciliacionExportColumnDto>(),
                    Array.Empty<IReadOnlyDictionary<string, object?>>());
            }

            List<ConciliacionExportColumnDto> columns = BuildColumns(projection.UiConfigDoc);
            List<ConciliacionDocument> conciliaciones =
                projection.ConciliacionesFiltradas ?? new List<ConciliacionDocument>();

            // DEBUG: si Columns.Count == 0, revisar UiConfigDoc.Fuente / Visible.
            _logger.LogInformation(
                "[ExportExcel][Compose] columns={ColumnCount} totalCount={TotalCount} docs={DocCount} reglaId={ReglaId} nombreRegla={NombreRegla}",
                columns.Count,
                projection.TotalCount,
                conciliaciones.Count,
                reglaId,
                nombreRegla);

            List<IReadOnlyDictionary<string, object?>> rows = new(conciliaciones.Count);
            foreach (ConciliacionDocument conc in conciliaciones)
            {
                rows.Add(MapRow(conc, columns));
            }

            decimal monto = TransformacionMontoReader.SumFromConciliaciones(
                conciliaciones.Select(c => c.ResultadoTransformacion),
                _montoCalculator);

            return new ConciliacionExportDataDto(
                nombreRegla,
                projection.TotalCount > 0 ? projection.TotalCount : conciliaciones.Count,
                monto,
                columns,
                rows);
        }

        private static List<ConciliacionExportColumnDto> BuildColumns(ParametrizacionUiDocument? uiConfig)
        {
            List<ConciliacionExportColumnDto> columns = new();
            if (uiConfig?.Fuente == null || uiConfig.Fuente.Count == 0)
            {
                return columns;
            }

            foreach (UiColumnDocument col in uiConfig.Fuente.Where(c => c.Visible).OrderBy(c => c.Order))
            {
                if (string.IsNullOrWhiteSpace(col.Field))
                {
                    continue;
                }

                string[] parts = col.Field.Split('.', 2);
                string propertyName = parts.Length == 2 ? parts[1] : col.Field;
                string? fuenteId = parts.Length == 2 ? parts[0] : null;

                // Excluir columna de acción (botones UI); estado SÍ se exporta.
                if (propertyName.Equals("accion", StringComparison.OrdinalIgnoreCase)
                    || col.Field.Equals("accion", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool isEstado = propertyName.Equals("estado", StringComparison.OrdinalIgnoreCase)
                    || col.Field.Equals("estado", StringComparison.OrdinalIgnoreCase);

                // RowKey estable: estado → estadoConciliacion (mismo criterio que ConciliationRowsComposer);
                // resto → PropertyName (valor dentro de Data[0] de la fuente).
                string rowKey = isEstado ? "estadoConciliacion" : propertyName;
                string? cellType = (col.CellConfig ?? col.Cell)?.Type;

                columns.Add(new ConciliacionExportColumnDto(
                    col.Header,
                    rowKey,
                    cellType,
                    isEstado ? null : fuenteId,
                    isEstado ? null : propertyName,
                    isEstado));
            }

            return columns;
        }

        private static IReadOnlyDictionary<string, object?> MapRow(
            ConciliacionDocument concDoc,
            List<ConciliacionExportColumnDto> columns)
        {
            Dictionary<string, object?> row = new Dictionary<string, object?>
            {
                // DEBUG: útil para cruzar fila Excel ↔ documento Mongo.
                ["conciliacionId"] = concDoc.ConciliacionId ?? string.Empty
            };

            Dictionary<string, Dictionary<string, object>> fuentesDict = BuildFuentesDict(concDoc);

            foreach (ConciliacionExportColumnDto column in columns)
            {
                if (column.IsEstado)
                {
                    row[column.RowKey] = concDoc.Estado ?? string.Empty;
                    continue;
                }

                row[column.RowKey] = ResolveFieldValue(fuentesDict, column.FuenteId, column.PropertyName);
            }

            return row;
        }

        private static Dictionary<string, Dictionary<string, object>> BuildFuentesDict(ConciliacionDocument concDoc)
        {
            Dictionary<string, Dictionary<string, object>> fuentesDict = new();
            if (concDoc.ResultadoFuentes == null)
            {
                return fuentesDict;
            }

            foreach (ResultadoFuenteDocument fDoc in concDoc.ResultadoFuentes)
            {
                if (fDoc.Data != null && fDoc.Data.Count > 0)
                {
                    // DEBUG: si un valor sale null, verificar que FuenteId del Field UI coincida con fDoc.FuenteId.
                    fuentesDict[fDoc.FuenteId ?? string.Empty] = fDoc.Data[0];
                }
            }

            return fuentesDict;
        }

        private static object? ResolveFieldValue(
            Dictionary<string, Dictionary<string, object>> fuentesDict,
            string? fuenteId,
            string? propertyName)
        {
            if (string.IsNullOrEmpty(fuenteId)
                || string.IsNullOrEmpty(propertyName)
                || !fuentesDict.TryGetValue(fuenteId, out Dictionary<string, object>? dataDoc)
                || !dataDoc.TryGetValue(propertyName, out object? raw)
                || raw == null)
            {
                return null;
            }

            return raw;
        }
    }
}
