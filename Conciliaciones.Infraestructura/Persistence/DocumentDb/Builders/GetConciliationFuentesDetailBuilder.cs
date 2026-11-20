using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class GetConciliationFuentesDetailBuilder
    {
        public static BsonDocument[] BuildAggregationPipeline(string conciliacionId)
        {
            // Datos reales usan "fallido" y "Fallido"; match case-insensitive vía $expr+$toLower.
            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { DocumentFields.Conciliacion.ConciliacionId, conciliacionId },
                    {
                        "$expr",
                        new BsonDocument("$in", new BsonArray
                        {
                            new BsonDocument("$toLower", new BsonDocument("$ifNull", new BsonArray
                            {
                                $"${DocumentFields.Conciliacion.Estado}",
                                ""
                            })),
                            new BsonArray(ConciliacionEstados.FallidosOErrorComparacion.ToArray())
                        })
                    }
                })
            };

            pipeline.AddRange(AggregationStages.LookupUnwind(
                DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                DocumentFields.Conciliacion.ParametrizacionId,
                DocumentFields.Parametrizacion.ParametrizacionId,
                DocumentFields.AggregationAliases.ParametrizacionDoc));
            pipeline.AddRange(AggregationStages.LookupUnwind(
                DocumentDBCollectionNames.MongoCollectionNames.SubProcesos,
                $"{DocumentFields.AggregationAliases.ParametrizacionDoc}.{DocumentFields.Parametrizacion.SubProcesoId}",
                DocumentFields.SubProceso.IdSubProceso,
                DocumentFields.AggregationAliases.SubprocesoDoc));
            pipeline.AddRange(AggregationStages.LookupUnwind(
                DocumentDBCollectionNames.MongoCollectionNames.Procesos,
                $"{DocumentFields.AggregationAliases.SubprocesoDoc}.{DocumentFields.SubProceso.IdProceso}",
                DocumentFields.Proceso.IdProceso,
                DocumentFields.AggregationAliases.ProcesoDoc));
            pipeline.AddRange(AggregationStages.LookupUnwind(
                DocumentDBCollectionNames.MongoCollectionNames.ParametrizacionUI,
                DocumentFields.Conciliacion.ParametrizacionId,
                DocumentFields.ParametrizacionUi.IdParametrizacion,
                DocumentFields.AggregationAliases.UiConfig));
            pipeline.Add(BuildFinalProjection());

            return pipeline.ToArray();
        }

        private static BsonDocument BuildFinalProjection() =>
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { DocumentFields.Conciliacion.ConciliacionId, $"${DocumentFields.Conciliacion.ConciliacionId}" },
                {
                    DocumentFields.AggregationAliases.NombreProceso,
                    new BsonDocument("$ifNull", new BsonArray
                    {
                        $"${DocumentFields.AggregationAliases.ProcesoDoc}.{DocumentFields.Proceso.Nombre}",
                        "Proceso"
                    })
                },
                { DocumentFields.Conciliacion.ResultadoFuentes, $"${DocumentFields.Conciliacion.ResultadoFuentes}" },
                { DocumentFields.AggregationAliases.UiConfigDoc, $"${DocumentFields.AggregationAliases.UiConfig}" }
            });
    }
}
