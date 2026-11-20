using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetResumenSubProcessByProcessBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_ConIdProcesoValido_GeneraPipelineBsonCorrecto()
        {
            // Arrange
            string idProcess = "PROC-777";

            // Act
            BsonDocument[] pipeline = GetResumenSubProcessByProcessBuilder.BuildAggregationPipeline(idProcess);

            // Assert
            Assert.IsNotNull(pipeline);
            Assert.AreEqual(4, pipeline.Length);

            // 1. Validar la etapa $match inicial
            BsonDocument firstStage = pipeline[0];
            Assert.IsTrue(firstStage.Contains("$match"));
            Assert.AreEqual(idProcess, firstStage["$match"]["idProceso"].AsString);

            // 2. Validar $lookup hacia parametrizacion
            BsonDocument lookupParam = pipeline[1];
            Assert.IsTrue(lookupParam.Contains("$lookup"));
            Assert.AreEqual("parametrizacion", lookupParam["$lookup"]["from"].AsString);
            Assert.AreEqual("idSubProceso", lookupParam["$lookup"]["localField"].AsString);
            Assert.AreEqual("subProcesoId", lookupParam["$lookup"]["foreignField"].AsString);

            // 3. Validar $lookup hacia conciliaciones
            BsonDocument lookupConciliaciones = pipeline[2];
            Assert.IsTrue(lookupConciliaciones.Contains("$lookup"));
            Assert.AreEqual("conciliaciones", lookupConciliaciones["$lookup"]["from"].AsString);

            // 4. Validar la etapa $project final
            BsonDocument lastStage = pipeline[3];
            Assert.IsTrue(lastStage.Contains("$project"));
            BsonDocument projectDoc = lastStage["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("idSubProceso"));
            Assert.IsTrue(projectDoc.Contains("nombreSubProceso"));
            Assert.IsTrue(projectDoc.Contains("conciliacionesFallidas"));
        }
    }
}