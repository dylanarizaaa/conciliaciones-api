using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services
{
    public sealed class UiConfigColumnExtractor : IUiConfigColumnExtractor
    {
        public List<UiColumnSpec> ExtractColumns(ParametrizacionUiDocument? uiConfig)
        {
            List<UiColumnSpec> columns = new List<UiColumnSpec>();
            if (uiConfig?.Fuente != null && uiConfig.Fuente.Count > 0)
            {
                columns.AddRange(uiConfig.Fuente.Select(UiColumnDocumentMapper.ToSpec));
                return columns;
            }

            if (uiConfig?.Blocks != null && uiConfig.Blocks.Count > 0)
            {
                List<UiColumnDocument>? legacyColumns = uiConfig.Blocks[0].Config?.Columns;
                if (legacyColumns != null)
                {
                    columns.AddRange(legacyColumns.Select(UiColumnDocumentMapper.ToSpec));
                }
            }

            return columns;
        }

        public List<(string FuenteId, string PropertyName)> ExtractVisibleFieldMappings(ParametrizacionUiDocument? uiConfig)
        {
            List<(string FuenteId, string PropertyName)> fieldMappings = new List<(string FuenteId, string PropertyName)>();
            if (uiConfig?.Fuente == null)
            {
                return fieldMappings;
            }

            foreach (UiColumnDocument col in uiConfig.Fuente.Where(c => c.Visible).OrderBy(c => c.Order))
            {
                if (string.IsNullOrEmpty(col.Field))
                {
                    continue;
                }

                string[] parts = col.Field.Split('.');
                fieldMappings.Add(parts.Length == 2
                    ? (parts[0], parts[1])
                    : (string.Empty, col.Field));
            }

            return fieldMappings;
        }
    }
}
