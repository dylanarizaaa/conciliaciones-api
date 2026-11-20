namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IDocumentDbIndexInitializer
    {
        Task EnsureIndexesAsync(CancellationToken cancellationToken);
    }
}
