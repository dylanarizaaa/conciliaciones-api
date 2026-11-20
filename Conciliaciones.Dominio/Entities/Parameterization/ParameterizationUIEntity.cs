using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Parameterization
{
    public sealed class ParameterizationUIEntity
    {
        private ParameterizationUIEntity(
            string idParametrizacion,
            List<UiColumnEntity> fuente,
            List<UiDatosBlockEntity> datos)
        {
            IdParametrizacion = idParametrizacion;
            Fuente = fuente;
            Datos = datos;
        }

        public string IdParametrizacion { get; }

        public IReadOnlyList<UiColumnEntity> Fuente { get; }

        public IReadOnlyList<UiDatosBlockEntity> Datos { get; }

        public static ParameterizationUIEntity Create(
            string idParametrizacion,
            IEnumerable<UiColumnEntity>? fuente = null,
            IEnumerable<UiDatosBlockEntity>? datos = null)
        {
            return new ParameterizationUIEntity(
                EntityValidation.Required(idParametrizacion, nameof(idParametrizacion)),
                fuente?.ToList() ?? new List<UiColumnEntity>(),
                datos?.ToList() ?? new List<UiDatosBlockEntity>());
        }
    }

    public sealed class UiColumnEntity
    {
        public UiColumnEntity(
            string field,
            string header,
            int order,
            bool sortable = true,
            bool visible = true,
            UiCellConfigEntity? cellConfig = null)
        {
            Field = EntityValidation.Required(field, nameof(field));
            Header = string.IsNullOrWhiteSpace(header) ? Field : header.Trim();
            Order = order;
            Sortable = sortable;
            Visible = visible;
            CellConfig = cellConfig ?? UiCellConfigEntity.Text();
        }

        public string Field { get; }

        public string Header { get; }

        public bool Sortable { get; }

        public bool Visible { get; }

        public int Order { get; }

        public UiCellConfigEntity CellConfig { get; }
    }

    public sealed class UiCellConfigEntity
    {
        private UiCellConfigEntity(
            string type,
            string? format,
            CurrencyCellConfigEntity? currency,
            IReadOnlyDictionary<string, string>? variantByValue)
        {
            Type = type;
            Format = format;
            Currency = currency;
            VariantByValue = variantByValue;
        }

        public string Type { get; }

        public string? Format { get; }

        public CurrencyCellConfigEntity? Currency { get; }

        public IReadOnlyDictionary<string, string>? VariantByValue { get; }

        public static UiCellConfigEntity Text() => new("text", null, null, null);

        public static UiCellConfigEntity CurrencyCop() =>
            new("currency", null, CurrencyCellConfigEntity.CopDefault(), null);

        public static UiCellConfigEntity DateIso() => new("date", "YYYY-MM-DD", null, null);

        public static UiCellConfigEntity Badge(IReadOnlyDictionary<string, string> variantByValue) =>
            new("badge", null, null, variantByValue);

        public static UiCellConfigEntity SelectRow() => new("select-row", null, null, null);

        public static UiCellConfigEntity FromType(string? tipoDato)
        {
            string tipo = string.IsNullOrWhiteSpace(tipoDato) ? "text" : tipoDato.Trim().ToLowerInvariant();
            return tipo switch
            {
                "currency" => CurrencyCop(),
                "date" => DateIso(),
                _ => new UiCellConfigEntity(tipo, null, null, null)
            };
        }
    }

    public sealed class CurrencyCellConfigEntity
    {
        public CurrencyCellConfigEntity(
            string currencyCode,
            string display,
            string digitsInfo,
            string locale,
            string symbol)
        {
            CurrencyCode = EntityValidation.Required(currencyCode, nameof(currencyCode));
            Display = EntityValidation.Required(display, nameof(display));
            DigitsInfo = EntityValidation.Required(digitsInfo, nameof(digitsInfo));
            Locale = EntityValidation.Required(locale, nameof(locale));
            Symbol = EntityValidation.Required(symbol, nameof(symbol));
        }

        public string CurrencyCode { get; }

        public string Display { get; }

        public string DigitsInfo { get; }

        public string Locale { get; }

        public string Symbol { get; }

        public static CurrencyCellConfigEntity CopDefault() =>
            new("COP", "symbol", "1.0-2", "es-CO", "$");
    }

    public sealed class UiDatosBlockEntity
    {
        public UiDatosBlockEntity(string idFuente, IEnumerable<UiColumnEntity> columns)
        {
            IdFuente = EntityValidation.Required(idFuente, nameof(idFuente));
            Columns = columns?.ToList() ?? new List<UiColumnEntity>();
        }

        public string IdFuente { get; }

        public IReadOnlyList<UiColumnEntity> Columns { get; }
    }
}
