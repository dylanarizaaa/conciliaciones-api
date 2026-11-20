using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetConciliationByIdSpBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_ConReglaId_GeneraPipelineBsonCorrecto()
        {
            string reglaId = "d75f5195-9109-4aea-a617-9c114dbc0443";

            BsonDocument[] pipeline = GetConciliationByIdSpBuilder.BuildAggregationPipeline(reglaId);

            Assert.IsNotNull(pipeline);
            Assert.AreEqual(7, pipeline.Length);

            BsonDocument stage1 = pipeline[0];
            Assert.IsTrue(stage1.Contains("$match"));
            Assert.IsFalse(stage1["$match"].AsBsonDocument.Contains("subProcesoId"));
            Assert.AreEqual(reglaId, stage1["$match"]["reglas.idRegla"].AsString);

            BsonDocument stage2 = pipeline[1];
            Assert.IsTrue(stage2.Contains("$addFields"));
            Assert.IsTrue(stage2["$addFields"].AsBsonDocument.Contains("reglaSeleccionada"));

            BsonDocument stage3 = pipeline[2];
            Assert.IsTrue(stage3.Contains("$lookup"));
            Assert.AreEqual("parametrizacionUI", stage3["$lookup"]["from"].AsString);

            BsonDocument projectStage = pipeline[6];
            Assert.IsTrue(projectStage.Contains("$project"));
            BsonDocument projectDoc = projectStage["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("uiConfigDoc"));
            Assert.IsTrue(projectDoc.Contains("conciliacionesFiltradas"));
        }
    }
}
