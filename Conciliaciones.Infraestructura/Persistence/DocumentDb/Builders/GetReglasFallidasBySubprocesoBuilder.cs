using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class GetReglasFallidasBySubprocesoBuilder
    {
        public static BsonDocument[] BuildDataPipeline(string subProcesoId, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            List<BsonDocument> pipeline = BuildBasePipeline(subProcesoId);
            pipeline.Add(new BsonDocument("$sort", new BsonDocument(DocumentFields.AggregationAliases.Casos, -1)));
            pipeline.Add(new BsonDocument("$skip", skip));
            pipeline.Add(new BsonDocument("$limit", pageSize));
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { DocumentFields.AggregationAliases.ReglaId, $"${DocumentFields.AggregationAliases.ReglaId}" },
                { DocumentFields.AggregationAliases.NombreRegla, $"${DocumentFields.AggregationAliases.NombreRegla}" },
                { DocumentFields.AggregationAliases.Casos, $"${DocumentFields.AggregationAliases.Casos}" },
                {
                    DocumentFields.AggregationAliases.ConciliacionesFiltradas,
                    FailedConciliationProjection.SlimArray($"${DocumentFields.AggregationAliases.ConciliacionesFiltradas}")
                }
            }));
            return pipeline.ToArray();
        }

        public static BsonDocument[] BuildTotalesPipeline(string subProcesoId)
        {
            List<BsonDocument> pipeline = BuildBasePipeline(subProcesoId);
            pipeline.Add(new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { DocumentFields.AggregationAliases.CasosTotales, new BsonDocument("$sum", $"${DocumentFields.AggregationAliases.Casos}") }
            }));
            return pipeline.ToArray();
        }

        private static List<BsonDocument> BuildBasePipeline(string subProcesoId) =>
            new List<BsonDocument>
            {
                AggregationStages.MatchEq(DocumentFields.Parametrizacion.SubProcesoId, subProcesoId),
                AggregationStages.Unwind($"${DocumentFields.Parametrizacion.Reglas}"),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones,
                    DocumentFields.Parametrizacion.ParametrizacionId,
                    DocumentFields.Conciliacion.ParametrizacionId,
                    DocumentFields.AggregationAliases.Conciliaciones),
                BuildConciliacionesFiltradasField(),
                BuildReglaProjection()
            };

        private static BsonDocument BuildConciliacionesFiltradasField() =>
            new BsonDocument("$addFields", new BsonDocument
            {
                {
                    DocumentFields.AggregationAliases.ConciliacionesFiltradas,
                    new BsonDocument("$filter", BuildFilterBody())
                }
            });

        private static BsonDocument BuildFilterBody() =>
            new BsonDocument
            {
                { "input", $"${DocumentFields.AggregationAliases.Conciliaciones}" },
                { "as", "c" },
                { "cond", new BsonDocument("$and", new BsonArray { BuildReglaMatch(), BuildEstadoMatch() }) }
            };

        private static BsonDocument BuildReglaMatch() =>
            new BsonDocument("$in", new BsonArray
            {
                $"${DocumentFields.Parametrizacion.Reglas}.{DocumentFields.Conciliacion.NombreRegla}",
                new BsonDocument("$cond", new BsonArray
                {
                    new BsonDocument("$isArray", $"$$c.{DocumentFields.Conciliacion.ResultadoRegla}"),
                    $"$$c.{DocumentFields.Conciliacion.ResultadoRegla}.{DocumentFields.Conciliacion.NombreRegla}",
                    new BsonArray { $"$$c.{DocumentFields.Conciliacion.ResultadoRegla}.{DocumentFields.Conciliacion.NombreRegla}" }
                })
            });

        private static BsonDocument BuildEstadoMatch() =>
            new BsonDocument("$in", new BsonArray
            {
                new BsonDocument("$toLower", new BsonDocument("$ifNull", new BsonArray
                {
                    $"$$c.{DocumentFields.Conciliacion.Estado}",
                    ""
                })),
                new BsonArray(ConciliacionEstados.FallidosOErrorComparacion.ToArray())
            });

        private static BsonDocument BuildReglaProjection() =>
            new BsonDocument("$project", new BsonDocument
            {
                {
                    DocumentFields.AggregationAliases.ReglaId,
                    new BsonDocument("$ifNull", new BsonArray
                    {
                        $"${DocumentFields.Parametrizacion.Reglas}.{DocumentFields.Reglas.IdRegla}",
                        string.Empty
                    })
                },
                {
                    DocumentFields.AggregationAliases.NombreRegla,
                    new BsonDocument("$ifNull", new BsonArray
                    {
                        $"${DocumentFields.Parametrizacion.Reglas}.{DocumentFields.Conciliacion.NombreRegla}",
                        $"${DocumentFields.Parametrizacion.ParametrizacionId}"
                    })
                },
                {
                    DocumentFields.AggregationAliases.Casos,
                    new BsonDocument("$size", $"${DocumentFields.AggregationAliases.ConciliacionesFiltradas}")
                },
                {
                    DocumentFields.AggregationAliases.ConciliacionesFiltradas,
                    $"${DocumentFields.AggregationAliases.ConciliacionesFiltradas}"
                }
            });
    }
}
