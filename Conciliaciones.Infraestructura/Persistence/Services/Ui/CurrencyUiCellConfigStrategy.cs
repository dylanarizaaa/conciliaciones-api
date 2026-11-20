using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services.Ui
{
    public sealed class CurrencyUiCellConfigStrategy : IUiCellConfigStrategy
    {
        public string Type => "currency";

        public void Apply(UiCellConfigDocument cellConfig)
        {
            CurrencyCellConfigEntity currency = CurrencyCellConfigEntity.CopDefault();
            cellConfig.Config = new Dictionary<string, object>
            {
                { DocumentFields.CurrencyConfig.CurrencyCode, currency.CurrencyCode },
                { DocumentFields.CurrencyConfig.Display, currency.Display },
                { DocumentFields.CurrencyConfig.DigitsInfo, currency.DigitsInfo },
                { DocumentFields.CurrencyConfig.Locale, currency.Locale },
                { DocumentFields.CurrencyConfig.Symbol, currency.Symbol }
            };
        }
    }
}
