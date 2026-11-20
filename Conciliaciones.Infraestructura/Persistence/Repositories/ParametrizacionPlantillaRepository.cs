using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public sealed class ParametrizacionPlantillaRepository : IParametrizacionPlantillaQueryPort
    {
        private readonly IDocumentDBContext _dbContext;

        public ParametrizacionPlantillaRepository(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<string?> GetPlantillaJsonAsync(string reglaId, string tipoBlock, CancellationToken cancellationToken)
        {
            BsonDocument[] pipeline =
            [
                AggregationStages.MatchEq($"{DocumentFields.Parametrizacion.Reglas}.{DocumentFields.Reglas.IdRegla}",reglaId),
                AggregationStages.Lookup(DocumentDBCollectionNames.MongoCollectionNames.ParametrizacionUI,DocumentFields.Parametrizacion.ParametrizacionId,DocumentFields.ParametrizacionUi.IdParametrizacion,"ui"),
                AggregationStages.Unwind("$ui", preserveNullAndEmptyArrays: false),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "plantilla", "$ui.plantilla" }
                })
            ];

            IMongoCollection<BsonDocument> collection = _dbContext.GetCollection<BsonDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion);

            using IAsyncCursor<PlantillaLookup> cursor = await collection.AggregateAsync<PlantillaLookup>(
                pipeline,
                cancellationToken: cancellationToken);

            PlantillaLookup? doc = await cursor.FirstOrDefaultAsync(cancellationToken);
            if (doc?.Plantilla == null || !doc.Plantilla.Contains(tipoBlock))
            {
                return null;
            }

            return doc.Plantilla[tipoBlock].ToJson();
        }

        [BsonIgnoreExtraElements]
        private sealed class PlantillaLookup
        {
            [BsonElement("plantilla")]
            public BsonDocument? Plantilla { get; set; }
        }
    }
}
