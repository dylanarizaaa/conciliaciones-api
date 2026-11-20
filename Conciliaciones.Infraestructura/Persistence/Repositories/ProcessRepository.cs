using Conciliaciones.Dominio.DTOs.Response.Process;
using Conciliaciones.Dominio.Ports.Interfaces.Process;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly IFailedConciliationSummaryMapper _summaryMapper;

        public ProcessRepository(IDocumentDBContext documentDBContext, IFailedConciliationSummaryMapper summaryMapper)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
            _summaryMapper = summaryMapper ?? throw new ArgumentNullException(nameof(summaryMapper));
        }

        public async Task<List<ProcesstSummaryDto>> GetProcessesByIdProjectsAsync(string proyectoId, CancellationToken cancellationToken)
        {
            IMongoCollection<ProcesoDocument> collection = _dbContext.GetCollection<ProcesoDocument>(DocumentDBCollectionNames.MongoCollectionNames.Procesos);
            BsonDocument[] pipeline = GetResumenProcesosByProyectoBuilder.BuildAggregationPipeline(proyectoId);

            using IAsyncCursor<ProcesoResumenProjection> cursor = await collection.AggregateAsync<ProcesoResumenProjection>(pipeline, null, cancellationToken);
            List<ProcesoResumenProjection> rawResults = await cursor.ToListAsync(cancellationToken);

            return rawResults.Select(doc =>
            {
                (long cantidad, decimal montoTotal) = _summaryMapper.Summarize(doc.ConciliacionesFallidas);
                return new ProcesstSummaryDto(doc.IdProceso, doc.NombreProceso, montoTotal, cantidad);
            }).ToList();
        }
    }
}
