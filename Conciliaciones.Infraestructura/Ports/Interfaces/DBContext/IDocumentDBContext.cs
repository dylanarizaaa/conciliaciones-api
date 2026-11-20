using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.DBContext
{
    public interface IDocumentDBContext
    {
        IMongoDatabase Database { get; }

        IMongoCollection<T> GetCollection<T>(string name);

        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
