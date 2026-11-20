using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDB
{
    public sealed class DocumentDbIndexInitializer : IDocumentDbIndexInitializer
    {
        private readonly IDocumentDBContext _context;

        public DocumentDbIndexInitializer(IDocumentDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
        {
            foreach ((string Collection, CreateIndexModel<BsonDocument>[] Indexes) definition in DocumentDbIndexCatalog.All)
            {
                await CreateAsync(definition.Collection, cancellationToken, definition.Indexes);
            }
        }

        private async Task CreateAsync(
            string collectionName,
            CancellationToken cancellationToken,
            params CreateIndexModel<BsonDocument>[] indexes)
        {
            IMongoCollection<BsonDocument> collection = _context.GetCollection<BsonDocument>(collectionName);
            await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
        }
    }

    internal static class DocumentDbIndexCatalog
    {
        public static IReadOnlyList<(string Collection, CreateIndexModel<BsonDocument>[] Indexes)> All { get; } =
            new List<(string, CreateIndexModel<BsonDocument>[])>
            {
                (DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion, ParametrizacionIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones, ConciliacionesIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.ParametrizacionUI, new[] { Asc("idParametrizacion") }),
                (DocumentDBCollectionNames.MongoCollectionNames.Proyectos, new[] { Asc("idProyecto"), Asc("nombre") }),
                (DocumentDBCollectionNames.MongoCollectionNames.Procesos, ProcesosIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.SubProcesos, SubProcesosIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.Automatizaciones, AutomatizacionesIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.Eventos, EventosIndexes()),
                (DocumentDBCollectionNames.MongoCollectionNames.HistorialConciliaciones, HistorialConciliacionesIndexes())
            };

        private static CreateIndexModel<BsonDocument>[] ParametrizacionIndexes() =>
            new[] { Asc("subProcesoId"), Asc("parametrizacionId"), Asc("reglas.idRegla") };

        private static CreateIndexModel<BsonDocument>[] ConciliacionesIndexes() =>
            new[]
            {
                Asc("parametrizacionId"),
                Asc("conciliacionId"),
                Asc("estado"),
                Compound("parametrizacionId", "estado")
            };

        private static CreateIndexModel<BsonDocument>[] ProcesosIndexes() =>
            new[] { Asc("idProyecto"), Asc("idProceso"), Compound("idProyecto", "nombre") };

        private static CreateIndexModel<BsonDocument>[] SubProcesosIndexes() =>
            new[] { Asc("idProceso"), Asc("idSubProceso"), Compound("idProceso", "nombre") };

        private static CreateIndexModel<BsonDocument>[] AutomatizacionesIndexes() =>
            new[]
            {
                Asc("automatizacionId"),
                UniqueAsc("parametrizacionId")
            };

        private static CreateIndexModel<BsonDocument>[] HistorialConciliacionesIndexes() =>
            new[]
            {
                Asc("conciliacionId"),
                Asc("eventoId"),
                Asc("auditoria.fechaHistorial")
            };

        private static CreateIndexModel<BsonDocument>[] EventosIndexes() =>
            new[]
            {
                Asc("eventoId"),
                Asc("conciliacionId"),
                Asc("parametrizacionId")
            };

        private static CreateIndexModel<BsonDocument> Asc(string field) =>
            new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending(field));

        private static CreateIndexModel<BsonDocument> UniqueAsc(string field) =>
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending(field),
                new CreateIndexOptions { Unique = true, Name = $"ux_{field}" });

        private static CreateIndexModel<BsonDocument> Compound(string field1, string field2) =>
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending(field1).Ascending(field2));
    }
}
