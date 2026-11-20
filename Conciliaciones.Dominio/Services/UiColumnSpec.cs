namespace Conciliaciones.Dominio.Services
{
    public sealed class UiColumnSpec
    {
        public UiColumnSpec(
            string field,
            string header,
            bool sortable,
            bool visible,
            int order,
            string? cellType,
            IReadOnlyDictionary<string, string>? variantByValue,
            IReadOnlyDictionary<string, object>? config)
        {
            Field = field;
            Header = header;
            Sortable = sortable;
            Visible = visible;
            Order = order;
            CellType = cellType;
            VariantByValue = variantByValue;
            Config = config;
        }

        public string Field { get; }

        public string Header { get; }

        public bool Sortable { get; }

        public bool Visible { get; }

        public int Order { get; }

        public string? CellType { get; }

        public IReadOnlyDictionary<string, string>? VariantByValue { get; }

        public IReadOnlyDictionary<string, object>? Config { get; }
    }
}
