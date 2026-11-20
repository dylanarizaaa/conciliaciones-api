using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services.Ui
{
    public sealed class UiCellConfigFactory
    {
        private readonly IReadOnlyDictionary<string, IUiCellConfigStrategy> _strategies;

        public UiCellConfigFactory(IEnumerable<IUiCellConfigStrategy> strategies)
        {
            _strategies = strategies?.ToDictionary(s => s.Type, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, IUiCellConfigStrategy>(StringComparer.OrdinalIgnoreCase);
        }

        public UiCellConfigFactory()
            : this(new IUiCellConfigStrategy[]
            {
                new CurrencyUiCellConfigStrategy(),
                new DateUiCellConfigStrategy()
            })
        {
        }

        public UiCellConfigDocument Create(string? tipoDato)
        {
            string tipoNormalizado = string.IsNullOrWhiteSpace(tipoDato) ? "text" : tipoDato.ToLowerInvariant();
            UiCellConfigDocument cellConfig = new UiCellConfigDocument { Type = tipoNormalizado };

            if (_strategies.TryGetValue(tipoNormalizado, out IUiCellConfigStrategy? strategy))
            {
                strategy.Apply(cellConfig);
            }

            return cellConfig;
        }
    }
}
