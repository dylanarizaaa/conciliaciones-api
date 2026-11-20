using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services.Ui
{
    public sealed class DateUiCellConfigStrategy : IUiCellConfigStrategy
    {
        public string Type => "date";

        public void Apply(UiCellConfigDocument cellConfig)
        {
            cellConfig.Format = "YYYY-MM-DD";
        }
    }
}
