using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class GetConciliationRowsSpBuilder
    {
        public static BsonDocument[] BuildAggregationPipeline(string reglaId, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            List<BsonDocument> pipeline = new List<BsonDocument>();
            pipeline.AddRange(AggregationStages.MatchReglaId(reglaId));
            pipeline.AddRange(AggregationStages.LookupUnwind(
                DocumentDBCollectionNames.MongoCollectionNames.ParametrizacionUI,
                DocumentFields.Parametrizacion.ParametrizacionId,
                DocumentFields.ParametrizacionUi.IdParametrizacion,
                DocumentFields.AggregationAliases.UiConfig));
            pipeline.Add(AggregationStages.Lookup(
                DocumentDBCollectionNames.MongoCollectionNames.Conciliaciones,
                DocumentFields.Parametrizacion.ParametrizacionId,
                DocumentFields.Conciliacion.ParametrizacionId,
                DocumentFields.AggregationAliases.Conciliaciones));
            pipeline.Add(AggregationStages.FilterFailedByReglaSeleccionada());
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { DocumentFields.AggregationAliases.UiConfigDoc, $"${DocumentFields.AggregationAliases.UiConfig}" },
                { DocumentFields.AggregationAliases.TotalCount, new BsonDocument("$size", $"${DocumentFields.AggregationAliases.ConciliacionesFiltradas}") },
                {
                    DocumentFields.AggregationAliases.ConciliacionesPaginadas,
                    new BsonDocument("$slice", new BsonArray
                    {
                        $"${DocumentFields.AggregationAliases.ConciliacionesFiltradas}",
                        skip,
                        pageSize
                    })
                }
            }));

            return pipeline.ToArray();
        }
    }
}
