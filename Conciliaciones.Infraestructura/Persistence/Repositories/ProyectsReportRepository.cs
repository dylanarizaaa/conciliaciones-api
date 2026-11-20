using Conciliaciones.Dominio.DTOs.Response.Projects;
using Conciliaciones.Dominio.Ports.Interfaces.Proyects;
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
    public class ProyectsReportRepository : IProyectsReportRepository
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly IFailedConciliationSummaryMapper _summaryMapper;

        public ProyectsReportRepository(IDocumentDBContext documentDBContext, IFailedConciliationSummaryMapper summaryMapper)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
            _summaryMapper = summaryMapper ?? throw new ArgumentNullException(nameof(summaryMapper));
        }

        public async Task<List<ProjectSummaryDto>> GetResumenProyectosAsync(CancellationToken cancellationToken)
        {
            IMongoCollection<ProyectoDocument> collection = _dbContext.GetCollection<ProyectoDocument>(
                DocumentDBCollectionNames.MongoCollectionNames.Proyectos);
            BsonDocument[] pipeline = GetResumenProyectosBuilder.BuildAggregationPipeline();

            using IAsyncCursor<ProyectoResumenProjection> cursor = await collection.AggregateAsync<ProyectoResumenProjection>(pipeline, null, cancellationToken);
            List<ProyectoResumenProjection> rawResults = await cursor.ToListAsync(cancellationToken);

            return rawResults.Select(doc =>
            {
                (long casosTotales, decimal montoTotal) = _summaryMapper.Summarize(doc.ConciliacionesFallidas);
                return new ProjectSummaryDto(doc.IdProyecto, doc.NombreProyecto, montoTotal, casosTotales);
            }).ToList();
        }
    }
}
