using Conciliaciones.Infraestructura.Persistence.Documents;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IUiCellConfigStrategy
    {
        string Type { get; }
        void Apply(UiCellConfigDocument cellConfig);
    }
}
