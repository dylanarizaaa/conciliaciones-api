using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Services;
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
    public class ConciliationQueryRepository : IConciliationQueryPort
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly ConciliationUiComposer _uiComposer;
        private readonly IUiConfigColumnExtractor _columnExtractor;
        private readonly IConciliationRowsComposer _rowsComposer;
        private readonly IConciliationFuentesDetailComposer _fuentesComposer;

        public ConciliationQueryRepository(
            IDocumentDBContext documentDBContext,
            ConciliationUiComposer uiComposer,
            IUiConfigColumnExtractor columnExtractor,
            IConciliationRowsComposer rowsComposer,
            IConciliationFuentesDetailComposer fuentesComposer)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
            _uiComposer = uiComposer ?? throw new ArgumentNullException(nameof(uiComposer));
            _columnExtractor = columnExtractor ?? throw new ArgumentNullException(nameof(columnExtractor));
            _rowsComposer = rowsComposer ?? throw new ArgumentNullException(nameof(rowsComposer));
            _fuentesComposer = fuentesComposer ?? throw new ArgumentNullException(nameof(fuentesComposer));
        }

        public async Task<ColumsDetailsDto> GetConciliationByIdSp(string reglaId, CancellationToken cancellationToken)
        {
            ConciliationUiAggregationProjection? groupResult = await AggregateFirstAsync<ParametrizacionDocument, ConciliationUiAggregationProjection>(
                DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                GetConciliationByIdSpBuilder.BuildAggregationPipeline(reglaId),
                cancellationToken);

            List<UiColumnSpec> columns = _columnExtractor.ExtractColumns(groupResult?.UiConfigDoc);
            return _uiComposer.ComposeDiscrepancyTable(columns);
        }

        public async Task<PagedRowsDto> GetConciliationRowsAsync(
            string reglaId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            ConciliationRowsAggregationProjection? groupResult =
                await AggregateFirstAsync<ParametrizacionDocument, ConciliationRowsAggregationProjection>(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                    GetConciliationRowsSpBuilder.BuildAggregationPipeline(reglaId, pageNumber, pageSize),
                    cancellationToken);

            return _rowsComposer.Compose(groupResult, pageNumber, pageSize);
        }

        public async Task<List<FuenteDetalleDto>> GetConciliationFuentesDetailAsync(string conciliacionId, CancellationToken cancellationToken)
        {
            ConciliationFuentesDetailProjection? docResult =
                await AggregateFirstAsync<ConciliacionDocument, ConciliationFuentesDetailProjection>(
                    DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones,
                    GetConciliationFuentesDetailBuilder.BuildAggregationPipeline(conciliacionId),
                    cancellationToken);

            return _fuentesComposer.Compose(docResult);
        }

        private async Task<TProjection?> AggregateFirstAsync<TDocument, TProjection>(
            string collectionName,
            BsonDocument[] pipeline,
            CancellationToken cancellationToken)
        {
            IMongoCollection<TDocument> collection = _dbContext.GetCollection<TDocument>(collectionName);
            using IAsyncCursor<TProjection> cursor =
                await collection.AggregateAsync<TProjection>(pipeline, null, cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
