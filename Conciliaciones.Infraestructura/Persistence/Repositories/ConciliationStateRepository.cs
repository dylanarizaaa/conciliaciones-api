using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public class ConciliationStateRepository : IConciliationStatePort
    {
        private readonly IDocumentDBContext _dbContext;

        public ConciliationStateRepository(IDocumentDBContext documentDBContext)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
        }
        private IMongoCollection<ConciliacionDocument> GetConciliacionesCollection() => _dbContext.GetCollection<ConciliacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones);
        private IMongoCollection<EventoDocument> GetEventosCollection() => _dbContext.GetCollection<EventoDocument>(DocumentDBCollectionNames.MongoCollectionNames.Eventos);
        private IMongoCollection<HistorialConciliacionesDocument> GetHistorialConciliacionesCollection() => _dbContext.GetCollection<HistorialConciliacionesDocument>(DocumentDBCollectionNames.MongoCollectionNames.HistorialConciliaciones);

        public async Task<StatusUpdateConciliation> UpdateEstadoConciliacionAsync(List<Guid> conciliacionId, CancellationToken cancellationToken)
        {

            List<Guid> distinctIds = conciliacionId.Distinct().ToList();
            List<string> idsAsString = distinctIds.Select(id => id.ToString()).ToList();
            IMongoCollection<ConciliacionDocument> collection = GetConciliacionesCollection();

            List<ConciliacionDocument> encontradas = await FindConciliacionesAsync(collection, idsAsString, cancellationToken);
            List<string> idsEncontrados = encontradas.Select(doc => doc.ConciliacionId).Distinct().ToList();
            List<Guid> idsEncontradosGuid = distinctIds.Where(id => idsEncontrados.Contains(id.ToString())).ToList();
            List<Guid> idsNoEncontrados = distinctIds.Except(idsEncontradosGuid).ToList();

            await CreateEventosAndUpdateConciliacionesAsync(collection, encontradas, cancellationToken);

            return new StatusUpdateConciliation(
                Actualizados: idsEncontradosGuid,
                NoEncontrados: idsNoEncontrados,
                TotalProcesados: distinctIds.Count);
        }


        private static async Task<List<ConciliacionDocument>> FindConciliacionesAsync(IMongoCollection<ConciliacionDocument> collection, List<string> idsAsString, CancellationToken cancellationToken)
        {
            FilterDefinition<ConciliacionDocument> filter = Builders<ConciliacionDocument>.Filter.In(x => x.ConciliacionId, idsAsString);

            using IAsyncCursor<ConciliacionDocument> cursor = await collection.FindAsync(filter, cancellationToken: cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
        }

        private async Task CreateEventosAndUpdateConciliacionesAsync(IMongoCollection<ConciliacionDocument> conciliaciones, List<ConciliacionDocument> encontradas, CancellationToken cancellationToken)
        {
            if (encontradas.Count == 0)
            {
                return;
            }

            DateTime fechaCambio = DateTime.UtcNow;
            List<string> conciliacionIds = encontradas.Select(c => c.ConciliacionId).Distinct().ToList();
            Dictionary<string, string> ultimoEventoPorConciliacion =
                await FindUltimoEventoIdPorConciliacionAsync(conciliacionIds, cancellationToken)
                ?? new Dictionary<string, string>();

            List<EventoDocument> eventos = new();
            List<HistorialConciliacionesDocument> historial = new();
            foreach (ConciliacionDocument conciliacion in encontradas)
            {
                EventoDocument evento = MapToEvento(conciliacion, fechaCambio);
                eventos.Add(evento);
                string eventoId = ultimoEventoPorConciliacion.TryGetValue(conciliacion.ConciliacionId, out string? existente)
                    && !string.IsNullOrWhiteSpace(existente)
                    ? existente
                    : evento.EventoId;
                historial.Add(MapToHistorial(conciliacion, eventoId, fechaCambio));
            }

            await GetEventosCollection().InsertManyAsync(eventos, cancellationToken: cancellationToken);
            await GetHistorialConciliacionesCollection().InsertManyAsync(historial, cancellationToken: cancellationToken);

            FilterDefinition<ConciliacionDocument> filter = Builders<ConciliacionDocument>.Filter.In(x => x.ConciliacionId, conciliacionIds);
            UpdateDefinition<ConciliacionDocument> update = Builders<ConciliacionDocument>.Update.Set(x => x.Estado, ConciliacionEstados.validando);
            await conciliaciones.UpdateManyAsync(filter, update, options: null, cancellationToken: cancellationToken);
        }

        private async Task<Dictionary<string, string>> FindUltimoEventoIdPorConciliacionAsync(List<string> conciliacionIds, CancellationToken cancellationToken)
        {
            Dictionary<string, string> resultado = new();
            if (conciliacionIds.Count == 0)
            {
                return resultado;
            }

            IMongoCollection<BsonDocument> historial = _dbContext.GetCollection<BsonDocument>(
                DocumentDBCollectionNames.MongoCollectionNames.HistorialConciliaciones);
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.In(
                DocumentFields.Conciliacion.ConciliacionId,
                conciliacionIds);
            ProjectionDefinition<BsonDocument> projection = Builders<BsonDocument>.Projection
                .Include(DocumentFields.Conciliacion.ConciliacionId)
                .Include(DocumentFields.HistorialConciliacion.EventoId)
                .Include(DocumentFields.Conciliacion.FechaCreacion)
                .Include($"{DocumentFields.HistorialConciliacion.Auditoria}.{DocumentFields.Auditoria.FechaHistorial}")
                .Exclude("_id");

            using IAsyncCursor<BsonDocument>? cursor = await historial.FindAsync(
                filter,
                new FindOptions<BsonDocument, BsonDocument> { Projection = projection },
                cancellationToken);
            if (cursor == null)
            {
                return resultado;
            }

            List<BsonDocument> documentos = await cursor.ToListAsync(cancellationToken) ?? new List<BsonDocument>();
            foreach (IGrouping<string, BsonDocument> grupo in documentos
                .Where(TieneEvento)
                .GroupBy(documento => documento[DocumentFields.Conciliacion.ConciliacionId].AsString))
            {
                BsonDocument ultimo = grupo.OrderBy(FechaReferencia).Last();
                resultado[grupo.Key] = ultimo[DocumentFields.HistorialConciliacion.EventoId].AsString;
            }

            return resultado;
        }

        private static bool TieneEvento(BsonDocument? documento)
        {
            return documento != null
                && documento.TryGetValue(DocumentFields.Conciliacion.ConciliacionId, out BsonValue conciliacionId)
                && conciliacionId.IsString
                && !string.IsNullOrWhiteSpace(conciliacionId.AsString)
                && documento.TryGetValue(DocumentFields.HistorialConciliacion.EventoId, out BsonValue eventoId)
                && eventoId.IsString
                && !string.IsNullOrWhiteSpace(eventoId.AsString);
        }

        private static DateTime FechaReferencia(BsonDocument documento)
        {
            if (documento.TryGetValue(DocumentFields.HistorialConciliacion.Auditoria, out BsonValue auditoria)
                && auditoria.IsBsonDocument
                && auditoria.AsBsonDocument.TryGetValue(DocumentFields.Auditoria.FechaHistorial, out BsonValue fechaHistorial)
                && fechaHistorial.IsValidDateTime)
            {
                return fechaHistorial.ToUniversalTime();
            }

            if (documento.TryGetValue(DocumentFields.Conciliacion.FechaCreacion, out BsonValue fechaCreacion)
                && fechaCreacion.IsValidDateTime)
            {
                return fechaCreacion.ToUniversalTime();
            }

            return DateTime.MinValue;
        }

        private static EventoDocument MapToEvento(ConciliacionDocument conciliacion, DateTime fechaCambio) =>
            new()
            {
                EventoId = Guid.NewGuid().ToString(),
                ParametrizacionId = conciliacion.ParametrizacionId,
                FechaCreacion = fechaCambio,
                Parametros = conciliacion.ParametrosEntrada ?? new Dictionary<string, object>(),
                ConciliacionId = conciliacion.ConciliacionId
            };

        private static HistorialConciliacionesDocument MapToHistorial(
            ConciliacionDocument conciliacion,
            string eventoId,
            DateTime fechaCambio) =>
            new()
            {
                ConciliacionId = conciliacion.ConciliacionId,
                ParametrizacionId = conciliacion.ParametrizacionId,
                FechaCreacion = DateTime.UtcNow,
                Estado = ConciliacionEstados.validando,
                ParametrosEntrada = conciliacion.ParametrosEntrada,
                ResultadoTransformacion = conciliacion.ResultadoTransformacion,
                ResultadoFuentes = conciliacion.ResultadoFuentes,
                ResultadoRegla = conciliacion.ResultadoRegla,
                EventoId = eventoId,
                Auditoria = new Auditoria
                {
                    UsuarioProceso = "web",
                    FechaHistorial = fechaCambio,
                    Origen = "WEB"
                }
            };
    }
}
