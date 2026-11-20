using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class GetResumenSubProcessByProcessBuilder
    {
        public static BsonDocument[] BuildAggregationPipeline(string idProcess)
        {
            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                AggregationStages.MatchEq(DocumentFields.SubProceso.IdProceso, idProcess),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                    DocumentFields.SubProceso.IdSubProceso,
                    DocumentFields.Parametrizacion.SubProcesoId,
                    DocumentFields.AggregationAliases.Parametrizaciones),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones,
                    $"{DocumentFields.AggregationAliases.Parametrizaciones}.{DocumentFields.Parametrizacion.ParametrizacionId}",
                    DocumentFields.Conciliacion.ParametrizacionId,
                    DocumentFields.AggregationAliases.Conciliaciones),
                BuildFailedProjection()
            };

            return pipeline.ToArray();
        }

        private static BsonDocument BuildFailedProjection() =>
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { DocumentFields.SubProceso.IdSubProceso, $"${DocumentFields.SubProceso.IdSubProceso}" },
                { DocumentFields.AggregationAliases.NombreSubProceso, $"${DocumentFields.SubProceso.Nombre}" },
                {
                    DocumentFields.AggregationAliases.ConciliacionesFallidas,
                    FailedConciliationProjection.SlimFilter(new BsonDocument("$filter", new BsonDocument
                    {
                        { "input", $"${DocumentFields.AggregationAliases.Conciliaciones}" },
                        { "as", "c" },
                        {
                            "cond",
                            new BsonDocument("$in", new BsonArray
                            {
                                new BsonDocument("$toLower", new BsonDocument("$ifNull", new BsonArray
                                {
                                    $"$$c.{DocumentFields.Conciliacion.Estado}",
                                    ""
                                })),
                                new BsonArray(ConciliacionEstados.FallidosOErrorComparacion.ToArray())
                            })
                        }
                    }))
                }
            });
    }
}
