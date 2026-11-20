using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy
{
    /// <summary>
    /// Plantilla Repository para Find/Create sobre DocumentDB.
    /// Las estrategias concretas solo definen filtro, extracción de Id y factory del documento.
    /// </summary>
    public abstract class MongoHierarchyEntityStrategyBase<TDocument> : IHierarchyEntityStrategy
    {
        private readonly IDocumentDBContext _dbContext;

        protected MongoHierarchyEntityStrategyBase(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public abstract HierarchyEntityKind Kind { get; }

        protected abstract string CollectionName { get; }

        protected abstract FilterDefinition<TDocument> BuildLookupFilter(string name, string? parentId);

        protected abstract FilterDefinition<TDocument> BuildIdFilter(string id);

        protected abstract string ExtractId(TDocument document);

        protected abstract TDocument CreateDocument(string newId, string name, string? parentId);

        public async Task<string?> FindExistingIdAsync(string name, string? parentId, CancellationToken cancellationToken)
        {
            IMongoCollection<TDocument> collection = _dbContext.GetCollection<TDocument>(CollectionName);
            FilterDefinition<TDocument> filter = BuildLookupFilter(name, parentId);
            TDocument? existing = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            return existing == null ? null : ExtractId(existing);
        }

        public async Task<bool> ExistsByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            IMongoCollection<TDocument> collection = _dbContext.GetCollection<TDocument>(CollectionName);
            return await collection.Find(BuildIdFilter(id)).AnyAsync(cancellationToken);
        }

        public async Task<string> CreateAsync(string name, string? parentId, CancellationToken cancellationToken)
        {
            string newId = Guid.NewGuid().ToString();
            TDocument document = CreateDocument(newId, name, parentId);
            IMongoCollection<TDocument> collection = _dbContext.GetCollection<TDocument>(CollectionName);
            await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
            return newId;
        }
    }
}
