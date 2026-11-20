using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
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
    public class SubProcessRepository : ISubProcessRepository
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly IFailedConciliationSummaryMapper _summaryMapper;

        public SubProcessRepository(IDocumentDBContext documentDBContext, IFailedConciliationSummaryMapper summaryMapper)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
            _summaryMapper = summaryMapper ?? throw new ArgumentNullException(nameof(summaryMapper));
        }

        public async Task<SubprocesoReglasResumenDto> GetReglasFallidasBySubprocesoIdAsync(string subProcesoId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            IMongoCollection<ParametrizacionDocument> collection = _dbContext.GetCollection<ParametrizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion);

            (List<ReglaFallidaProjection> rawDataList, ReglasFallidasTotalesProjection? totalDoc) = await AggregateReglasAsync(collection, subProcesoId, pageNumber, pageSize, cancellationToken);

            long casosTotales = totalDoc?.CasosTotales ?? 0;
            (List<FailedRuleDto> dataList, decimal montoAfectadoTotal) = MapFailedRules(rawDataList);
            long totalPages = pageSize > 0 ? (long)Math.Ceiling((double)casosTotales / pageSize) : 0;

            return new SubprocesoReglasResumenDto(
                casosTotales,
                montoAfectadoTotal,
                pageNumber,
                pageSize,
                totalPages,
                dataList);
        }

        public async Task<List<SubProcessInfoDto>> GetSubProcesosByIdProcess(string idProcess, CancellationToken cancellationToken)
        {
            IMongoCollection<SubProcesoDocument> collection = _dbContext.GetCollection<SubProcesoDocument>(
                DocumentDBCollectionNames.MongoCollectionNames.SubProcesos);
            BsonDocument[] pipeline = GetResumenSubProcessByProcessBuilder.BuildAggregationPipeline(idProcess);

            using IAsyncCursor<SubProcesoResumenProjection> cursor =
                await collection.AggregateAsync<SubProcesoResumenProjection>(pipeline, null, cancellationToken);
            List<SubProcesoResumenProjection> documents = await cursor.ToListAsync(cancellationToken);

            return documents.Select(doc =>
            {
                (long cantidad, decimal montoTotal) = _summaryMapper.Summarize(doc.ConciliacionesFallidas);
                return new SubProcessInfoDto(doc.IdSubProceso, doc.NombreSubProceso, montoTotal, cantidad);
            }).ToList();
        }

        private static async Task<(List<ReglaFallidaProjection> Data, ReglasFallidasTotalesProjection? Totales)> AggregateReglasAsync(IMongoCollection<ParametrizacionDocument> collection, string subProcesoId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            BsonDocument[] pipelineData = GetReglasFallidasBySubprocesoBuilder.BuildDataPipeline(subProcesoId, pageNumber, pageSize);
            BsonDocument[] pipelineTotales = GetReglasFallidasBySubprocesoBuilder.BuildTotalesPipeline(subProcesoId);

            Task<IAsyncCursor<ReglaFallidaProjection>> dataTask = collection.AggregateAsync<ReglaFallidaProjection>(pipelineData, null, cancellationToken);
            Task<IAsyncCursor<ReglasFallidasTotalesProjection>> totalesTask = collection.AggregateAsync<ReglasFallidasTotalesProjection>(pipelineTotales, null, cancellationToken);
            await Task.WhenAll(dataTask, totalesTask);
            using IAsyncCursor<ReglaFallidaProjection> cursorData = await dataTask;
            using IAsyncCursor<ReglasFallidasTotalesProjection> cursorTotales = await totalesTask;

            List<ReglaFallidaProjection> rawDataList = await cursorData.ToListAsync(cancellationToken);
            ReglasFallidasTotalesProjection? totalDoc = await cursorTotales.FirstOrDefaultAsync(cancellationToken);
            return (rawDataList, totalDoc);
        }

        private (List<FailedRuleDto> Data, decimal MontoTotal) MapFailedRules(List<ReglaFallidaProjection> rawDataList)
        {
            decimal montoAfectadoTotal = 0m;
            List<FailedRuleDto> dataList = new List<FailedRuleDto>();

            foreach (ReglaFallidaProjection doc in rawDataList)
            {
                string idRegla = string.IsNullOrWhiteSpace(doc.IdRegla) ? string.Empty : doc.IdRegla;
                string nombre = string.IsNullOrWhiteSpace(doc.NombreRegla) ? "Sin Nombre" : doc.NombreRegla;
                decimal montoRegla = _summaryMapper.SumMontos(doc.ConciliacionesFiltradas);
                montoAfectadoTotal += montoRegla;
                dataList.Add(new FailedRuleDto(idRegla, nombre, doc.Casos, montoRegla));
            }

            return (dataList, montoAfectadoTotal);
        }
    }
}
