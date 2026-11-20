using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Dominio.Services
{
    public sealed class ConciliationUiComposer
    {
        public ColumsDetailsDto ComposeDiscrepancyTable(IEnumerable<UiColumnSpec> columns)
        {
            List<ColumnUiDto> columnsList = (columns ?? Enumerable.Empty<UiColumnSpec>())
                .Where(c => c.Visible)
                .OrderBy(c => c.Order)
                .Select(MapColumn)
                .ToList();

            TableConfigsDto tableConfig = new TableConfigsDto(columnsList);
            BlockUiDto blockUi = new BlockUiDto("discrepancias-table", "table", "Diferencias detectadas", tableConfig);
            return new ColumsDetailsDto(new List<BlockUiDto> { blockUi });
        }

        public ColumnUiDto MapColumn(UiColumnSpec spec)
        {
            CellConfigDto? cellConfig = null;
            if (!string.IsNullOrWhiteSpace(spec.CellType) || spec.VariantByValue != null || spec.Config != null)
            {
                Dictionary<string, string>? variants = spec.VariantByValue == null
                    ? null
                    : new Dictionary<string, string>(spec.VariantByValue);
                Dictionary<string, object>? config = spec.Config == null
                    ? null
                    : new Dictionary<string, object>(spec.Config);
                cellConfig = new CellConfigDto(spec.CellType ?? "text", variants, config);
            }

            return new ColumnUiDto(spec.Field, spec.Header, spec.Sortable, cellConfig);
        }
    }
}
