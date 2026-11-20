using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetResumenProcesosByProyectoBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_ConIdProyectoValido_GeneraPipelineBsonCorrecto()
        {
            // Arrange
            string idProyecto = "PROY-888";

            // Act
            BsonDocument[] pipeline = GetResumenProcesosByProyectoBuilder.BuildAggregationPipeline(idProyecto);

            // Assert
            Assert.IsNotNull(pipeline);
            Assert.AreEqual(5, pipeline.Length);

            // 1. Validar la etapa $match inicial
            BsonDocument firstStage = pipeline[0];
            Assert.IsTrue(firstStage.Contains("$match"));
            Assert.AreEqual(idProyecto, firstStage["$match"]["idProyecto"].AsString);

            // 2. Validar los $lookup intermedios
            BsonDocument lookupSubprocesos = pipeline[1];
            Assert.AreEqual("subprocesos", lookupSubprocesos["$lookup"]["from"].AsString);

            BsonDocument lookupParametrizacion = pipeline[2];
            Assert.AreEqual("parametrizacion", lookupParametrizacion["$lookup"]["from"].AsString);

            BsonDocument lookupConciliaciones = pipeline[3];
            Assert.AreEqual("conciliaciones", lookupConciliaciones["$lookup"]["from"].AsString);

            // 3. Validar la etapa $project final
            BsonDocument lastStage = pipeline[4];
            Assert.IsTrue(lastStage.Contains("$project"));
            BsonDocument projectDoc = lastStage["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("idProceso"));
            Assert.IsTrue(projectDoc.Contains("nombreProceso"));
            Assert.IsTrue(projectDoc.Contains("conciliacionesFallidas"));
        }
    }
}