using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IUiConfigColumnExtractor
    {
        List<UiColumnSpec> ExtractColumns(ParametrizacionUiDocument? uiConfig);
        List<(string FuenteId, string PropertyName)> ExtractVisibleFieldMappings(ParametrizacionUiDocument? uiConfig);
    }
}
