using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetConciliationRowsSpBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_ConParametrosPrimeraPagina_GeneraPipelineBsonCorrecto()
        {
            string reglaId = "d75f5195-9109-4aea-a617-9c114dbc0443";
            int pageNumber = 1;
            int pageSize = 10;
            int skipEsperado = 0;

            BsonDocument[] pipeline = GetConciliationRowsSpBuilder.BuildAggregationPipeline(reglaId, pageNumber, pageSize);

            Assert.IsNotNull(pipeline);
            Assert.AreEqual(7, pipeline.Length);

            BsonDocument stage1 = pipeline[0];
            Assert.IsTrue(stage1.Contains("$match"));
            Assert.IsFalse(stage1["$match"].AsBsonDocument.Contains("subProcesoId"));
            Assert.AreEqual(reglaId, stage1["$match"]["reglas.idRegla"].AsString);

            BsonDocument projectStage = pipeline[6];
            Assert.IsTrue(projectStage.Contains("$project"));
            BsonDocument projectDoc = projectStage["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("uiConfigDoc"));
            Assert.IsTrue(projectDoc.Contains("totalCount"));
            Assert.IsTrue(projectDoc.Contains("conciliacionesPaginadas"));

            BsonDocument sliceDoc = projectDoc["conciliacionesPaginadas"].AsBsonDocument;
            Assert.IsTrue(sliceDoc.Contains("$slice"));
            BsonArray sliceArray = sliceDoc["$slice"].AsBsonArray;
            Assert.AreEqual(skipEsperado, sliceArray[1].AsInt32);
            Assert.AreEqual(pageSize, sliceArray[2].AsInt32);
        }

        [TestMethod]
        public void BuildAggregationPipeline_ConSegundaPagina_CalculaSkipCorrectamente()
        {
            string reglaId = "d75f5195-9109-4aea-a617-9c114dbc0443";
            int pageNumber = 2;
            int pageSize = 15;
            int skipEsperado = 15;

            BsonDocument[] pipeline = GetConciliationRowsSpBuilder.BuildAggregationPipeline(reglaId, pageNumber, pageSize);

            Assert.IsNotNull(pipeline);
            Assert.AreEqual(7, pipeline.Length);

            BsonDocument projectStage = pipeline[6];
            Assert.IsTrue(projectStage.Contains("$project"));
            BsonDocument projectDoc = projectStage["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("conciliacionesPaginadas"));

            BsonDocument sliceDoc = projectDoc["conciliacionesPaginadas"].AsBsonDocument;
            BsonArray sliceArray = sliceDoc["$slice"].AsBsonArray;
            Assert.AreEqual(skipEsperado, sliceArray[1].AsInt32);
            Assert.AreEqual(pageSize, sliceArray[2].AsInt32);
        }
    }
}
