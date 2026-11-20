using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class GetResumenProcesosByProyectoBuilder
    {
        public static BsonDocument[] BuildAggregationPipeline(string idProyecto)
        {
            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                AggregationStages.MatchEq(DocumentFields.Proceso.IdProyecto, idProyecto),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.SubProcesos,
                    DocumentFields.Proceso.IdProceso,
                    DocumentFields.SubProceso.IdProceso,
                    DocumentFields.AggregationAliases.SubProcesos),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                    $"{DocumentFields.AggregationAliases.SubProcesos}.{DocumentFields.SubProceso.IdSubProceso}",
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
                { DocumentFields.Proceso.IdProceso, $"${DocumentFields.Proceso.IdProceso}" },
                { DocumentFields.AggregationAliases.NombreProceso, $"${DocumentFields.Proceso.Nombre}" },
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
