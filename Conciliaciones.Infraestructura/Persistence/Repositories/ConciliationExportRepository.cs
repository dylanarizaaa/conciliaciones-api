using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public sealed class ConciliationExportRepository : IConciliationExportQueryPort
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly IConciliationExportComposer _exportComposer;
        private readonly ILogger<ConciliationExportRepository> _logger;

        public ConciliationExportRepository(
            IDocumentDBContext dbContext,
            IConciliationExportComposer exportComposer,
            ILogger<ConciliationExportRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _exportComposer = exportComposer ?? throw new ArgumentNullException(nameof(exportComposer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConciliacionExportDataDto> GetExportDataAsync(
            string reglaId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[ExportExcel][Repo] Inicio aggregate. reglaId={ReglaId}",
                reglaId);

            BsonDocument[] pipeline = GetConciliationExportBuilder.BuildAggregationPipeline(reglaId);

            IMongoCollection<ParametrizacionDocument> collection =
                _dbContext.GetCollection<ParametrizacionDocument>(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion);

            using IAsyncCursor<ConciliationExportAggregationProjection> cursor =
                await collection.AggregateAsync<ConciliationExportAggregationProjection>(
                    pipeline,
                    cancellationToken: cancellationToken);

            ConciliationExportAggregationProjection? projection =
                await cursor.FirstOrDefaultAsync(cancellationToken);

            _logger.LogInformation(
                "[ExportExcel][Repo] Aggregate OK. found={Found} totalCount={TotalCount}",
                projection != null,
                projection?.TotalCount ?? 0);

            return _exportComposer.Compose(projection, reglaId);
        }
    }
}
