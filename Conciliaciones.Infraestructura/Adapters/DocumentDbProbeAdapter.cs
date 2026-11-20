using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Adapters
{
    public sealed class DocumentDbProbeAdapter : IDatabaseProbe
    {
        private readonly IDocumentDBContext _dbContext;

        public DocumentDbProbeAdapter(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<DatabasePingResult> PingAsync(CancellationToken cancellationToken)
        {
            try
            {
                BsonDocument command = new BsonDocument("ping", 1);
                BsonDocument result = await _dbContext.Database.RunCommandAsync<BsonDocument>(
                    command,
                    cancellationToken: cancellationToken);
                IAsyncCursor<string> collectionsCursor = await _dbContext.Database.ListCollectionNamesAsync(
                    cancellationToken: cancellationToken);
                List<string> collections = await collectionsCursor.ToListAsync(cancellationToken);

                return new DatabasePingResult(
                    true,
                    "Conectado exitosamente a MongoDB/DocumentDB",
                    result.ToString(),
                    _dbContext.Database.DatabaseNamespace.DatabaseName,
                    collections,
                    null,
                    null);
            }
            catch (Exception ex)
            {
                return new DatabasePingResult(
                    false,
                    "Error de conexión con MongoDB",
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>(),
                    ex.Message,
                    ex.InnerException?.Message);
            }
        }

        public async Task<DatabaseCatalogResult> ListDatabasesAsync(CancellationToken cancellationToken)
        {
            try
            {
                IAsyncCursor<string> dbNamesCursor = await _dbContext.Database.Client.ListDatabaseNamesAsync(cancellationToken);
                List<string> dbNames = await dbNamesCursor.ToListAsync(cancellationToken);
                return new DatabaseCatalogResult(
                    true,
                    "OK",
                    dbNames,
                    _dbContext.Database.DatabaseNamespace.DatabaseName,
                    null,
                    null);
            }
            catch (Exception ex)
            {
                return new DatabaseCatalogResult(
                    false,
                    "Error al listar las bases de datos (Posible falta de permisos del usuario)",
                    Array.Empty<string>(),
                    string.Empty,
                    ex.Message,
                    ex.InnerException?.Message);
            }
        }
    }
}
