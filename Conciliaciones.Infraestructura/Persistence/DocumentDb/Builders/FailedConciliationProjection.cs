using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class FailedConciliationProjection
    {
        public static BsonDocument SlimFilter(BsonDocument filterStage) => Slim(filterStage);

        public static BsonDocument SlimArray(string inputField) => Slim(inputField);

        private static BsonDocument Slim(BsonValue input) =>
            new BsonDocument("$map", new BsonDocument
            {
                { "input", input },
                { "as", "c" },
                {
                    "in",
                    new BsonDocument
                    {
                        {
                            DocumentFields.Conciliacion.ResultadoTransformacion,
                            $"$$c.{DocumentFields.Conciliacion.ResultadoTransformacion}"
                        }
                    }
                }
            });
    }
}
