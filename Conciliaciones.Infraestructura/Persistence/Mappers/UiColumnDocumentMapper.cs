using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;

namespace Conciliaciones.Infraestructura.Persistence.Mappers
{
    public static class UiColumnDocumentMapper
    {
        public static UiColumnSpec ToSpec(UiColumnDocument column)
        {
            UiCellConfigDocument? cell = column.CellConfig ?? column.Cell;
            return new UiColumnSpec(
                column.Field,
                column.Header,
                column.Sortable,
                column.Visible,
                column.Order,
                cell?.Type,
                cell?.VariantByValue,
                cell?.Config);
        }
    }
}
