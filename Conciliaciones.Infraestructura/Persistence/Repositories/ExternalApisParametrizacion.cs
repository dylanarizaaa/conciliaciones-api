using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Dominio.Ports.Interfaces.ExternalAPIs;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public sealed class ExternalApisParametrizacion : IExternalAPIsRepository
    {
        private readonly IDocumentDBContext _dbContext;

        public ExternalApisParametrizacion(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<RulesParamsResponseDto>> GetParameterizationsByClientAsync(
            string? idProyecto,
            string? idProceso,
            string? idSubProceso,
            CancellationToken cancellationToken)
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                idProyecto,
                idProceso,
                idSubProceso);

            // El pipeline arranca en parametrizacion, subprocesos o procesos según el filtro más específico.
            string collectionName = ResolveRootCollection(idProyecto, idProceso, idSubProceso);
            IMongoCollection<BsonDocument> collection = _dbContext.GetCollection<BsonDocument>(collectionName);

            using IAsyncCursor<ParametrizacionRulesParamsProjection> cursor =
                await collection.AggregateAsync<ParametrizacionRulesParamsProjection>(
                    pipeline,
                    cancellationToken: cancellationToken);

            List<ParametrizacionRulesParamsProjection> docs = await cursor.ToListAsync(cancellationToken);
            return docs.Select(Map).ToList();
        }

        private static string ResolveRootCollection(string? idProyecto, string? idProceso, string? idSubProceso)
        {
            if (!string.IsNullOrWhiteSpace(idSubProceso))
            {
                return DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion;
            }

            if (!string.IsNullOrWhiteSpace(idProceso))
            {
                return DocumentDBCollectionNames.MongoCollectionNames.SubProcesos;
            }

            return DocumentDBCollectionNames.MongoCollectionNames.Procesos;
        }

        private static RulesParamsResponseDto Map(ParametrizacionRulesParamsProjection doc)
        {
            List<Dictionary<string, string>>? parametros = doc.Parametros?
                .Select(p => new Dictionary<string, string>
                {
                    ["nombre"] = p.Nombre ?? string.Empty,
                    ["tipo"] = p.Tipo ?? string.Empty
                })
                .ToList();

            List<RuleParamDto> reglas = doc.Reglas?
                .Select(r => new RuleParamDto(r.IdRegla, r.NombreRegla))
                .ToList()
                ?? new List<RuleParamDto>();

            return new RulesParamsResponseDto(doc.ParametrizacionId, parametros, reglas);
        }
    }
}
