using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    /// <summary>
    /// Un solo pipeline de lectura: prioriza subProcesoId → procesoId → proyectoId.
    /// </summary>
    public static class GetParameterizationsByClientBuilder
    {
        public static BsonDocument[] BuildAggregationPipeline(
            string? idProyecto,
            string? idProceso,
            string? idSubProceso)
        {
            if (!string.IsNullOrWhiteSpace(idSubProceso))
            {
                return BuildFromSubProceso(idSubProceso.Trim(), idProceso, idProyecto);
            }

            if (!string.IsNullOrWhiteSpace(idProceso))
            {
                return BuildFromProceso(idProceso.Trim(), idProyecto);
            }

            return BuildFromProyecto(idProyecto!.Trim());
        }

        private static BsonDocument[] BuildFromSubProceso(
            string idSubProceso,
            string? idProceso,
            string? idProyecto)
        {
            List<BsonDocument> pipeline = new()
            {
                AggregationStages.MatchEq(DocumentFields.Parametrizacion.SubProcesoId, idSubProceso)
            };

            // Si también envían proceso/proyecto, validar pertenencia vía lookup.
            if (!string.IsNullOrWhiteSpace(idProceso) || !string.IsNullOrWhiteSpace(idProyecto))
            {
                pipeline.AddRange(AggregationStages.LookupUnwind(
                    DocumentDBCollectionNames.MongoCollectionNames.SubProcesos,
                    DocumentFields.Parametrizacion.SubProcesoId,
                    DocumentFields.SubProceso.IdSubProceso,
                    DocumentFields.AggregationAliases.SubprocesoDoc,
                    preserveNullAndEmptyArrays: false));

                if (!string.IsNullOrWhiteSpace(idProceso))
                {
                    pipeline.Add(AggregationStages.MatchEq(
                        $"{DocumentFields.AggregationAliases.SubprocesoDoc}.{DocumentFields.SubProceso.IdProceso}",
                        idProceso.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(idProyecto))
                {
                    pipeline.AddRange(AggregationStages.LookupUnwind(
                        DocumentDBCollectionNames.MongoCollectionNames.Procesos,
                        $"{DocumentFields.AggregationAliases.SubprocesoDoc}.{DocumentFields.SubProceso.IdProceso}",
                        DocumentFields.Proceso.IdProceso,
                        DocumentFields.AggregationAliases.ProcesoDoc,
                        preserveNullAndEmptyArrays: false));
                    pipeline.Add(AggregationStages.MatchEq(
                        $"{DocumentFields.AggregationAliases.ProcesoDoc}.{DocumentFields.Proceso.IdProyecto}",
                        idProyecto.Trim()));
                }
            }

            pipeline.Add(BuildRulesParamsProject());
            return pipeline.ToArray();
        }

        private static BsonDocument[] BuildFromProceso(string idProceso, string? idProyecto)
        {
            List<BsonDocument> pipeline = new()
            {
                AggregationStages.MatchEq(DocumentFields.SubProceso.IdProceso, idProceso)
            };

            if (!string.IsNullOrWhiteSpace(idProyecto))
            {
                pipeline.AddRange(AggregationStages.LookupUnwind(
                    DocumentDBCollectionNames.MongoCollectionNames.Procesos,
                    DocumentFields.SubProceso.IdProceso,
                    DocumentFields.Proceso.IdProceso,
                    DocumentFields.AggregationAliases.ProcesoDoc,
                    preserveNullAndEmptyArrays: false));
                pipeline.Add(AggregationStages.MatchEq(
                    $"{DocumentFields.AggregationAliases.ProcesoDoc}.{DocumentFields.Proceso.IdProyecto}",
                    idProyecto.Trim()));
            }

            pipeline.Add(AggregationStages.Lookup(
                DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                DocumentFields.SubProceso.IdSubProceso,
                DocumentFields.Parametrizacion.SubProcesoId,
                DocumentFields.AggregationAliases.Parametrizaciones));
            pipeline.Add(AggregationStages.Unwind(
                $"${DocumentFields.AggregationAliases.Parametrizaciones}",
                preserveNullAndEmptyArrays: false));
            pipeline.Add(BuildRulesParamsProjectFromUnwind());
            return pipeline.ToArray();
        }

        private static BsonDocument[] BuildFromProyecto(string idProyecto)
        {
            List<BsonDocument> pipeline = new()
            {
                AggregationStages.MatchEq(DocumentFields.Proceso.IdProyecto, idProyecto),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.SubProcesos,
                    DocumentFields.Proceso.IdProceso,
                    DocumentFields.SubProceso.IdProceso,
                    DocumentFields.AggregationAliases.SubProcesos),
                AggregationStages.Unwind(
                    $"${DocumentFields.AggregationAliases.SubProcesos}",
                    preserveNullAndEmptyArrays: false),
                AggregationStages.Lookup(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion,
                    $"{DocumentFields.AggregationAliases.SubProcesos}.{DocumentFields.SubProceso.IdSubProceso}",
                    DocumentFields.Parametrizacion.SubProcesoId,
                    DocumentFields.AggregationAliases.Parametrizaciones),
                AggregationStages.Unwind(
                    $"${DocumentFields.AggregationAliases.Parametrizaciones}",
                    preserveNullAndEmptyArrays: false),
                BuildRulesParamsProjectFromUnwind()
            };

            return pipeline.ToArray();
        }

        private static BsonDocument BuildRulesParamsProject() =>
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { DocumentFields.Parametrizacion.ParametrizacionId, $"${DocumentFields.Parametrizacion.ParametrizacionId}" },
                { DocumentFields.Parametrizacion.Parametros, $"${DocumentFields.Parametrizacion.Parametros}" },
                { DocumentFields.Parametrizacion.Reglas, $"${DocumentFields.Parametrizacion.Reglas}" }
            });

        private static BsonDocument BuildRulesParamsProjectFromUnwind() =>
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                {
                    DocumentFields.Parametrizacion.ParametrizacionId,
                    $"${DocumentFields.AggregationAliases.Parametrizaciones}.{DocumentFields.Parametrizacion.ParametrizacionId}"
                },
                {
                    DocumentFields.Parametrizacion.Parametros,
                    $"${DocumentFields.AggregationAliases.Parametrizaciones}.{DocumentFields.Parametrizacion.Parametros}"
                },
                {
                    DocumentFields.Parametrizacion.Reglas,
                    $"${DocumentFields.AggregationAliases.Parametrizaciones}.{DocumentFields.Parametrizacion.Reglas}"
                }
            });
    }
}
