using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetReglasFallidasBySubprocesoBuilderTests
    {
        [TestMethod]
        public void BuildDataPipeline_ConPaginacionValida_GeneraPipelineBsonPaginadoCorrectamente()
        {
            // Arrange
            string subProcesoId = "SP-550";
            int pageNumber = 2;
            int pageSize = 10;
            int skipEsperado = 10; // (2 - 1) * 10

            // Act
            BsonDocument[] pipeline = GetReglasFallidasBySubprocesoBuilder.BuildDataPipeline(subProcesoId, pageNumber, pageSize);

            // Assert
            Assert.IsNotNull(pipeline);
            Assert.AreEqual(9, pipeline.Length); // 5 base + 4 (sort, skip, limit, project)

            // Validar $match inicial
            BsonDocument firstStage = pipeline[0];
            Assert.IsTrue(firstStage.Contains("$match"));
            Assert.AreEqual(subProcesoId, firstStage["$match"]["subProcesoId"].AsString);

            // Array plano de reglas (no reglas.reglas / reglasAsociadas)
            BsonDocument unwindStage = pipeline[1];
            Assert.IsTrue(unwindStage.Contains("$unwind"));
            Assert.AreEqual("$reglas", unwindStage["$unwind"]["path"].AsString);

            // Validar $sort
            BsonDocument sortStage = pipeline[5];
            Assert.IsTrue(sortStage.Contains("$sort"));
            Assert.AreEqual(-1, sortStage["$sort"]["casos"].AsInt32);

            // Validar $skip y $limit
            BsonDocument skipStage = pipeline[6];
            Assert.IsTrue(skipStage.Contains("$skip"));
            Assert.AreEqual(skipEsperado, skipStage["$skip"].AsInt32);

            BsonDocument limitStage = pipeline[7];
            Assert.IsTrue(limitStage.Contains("$limit"));
            Assert.AreEqual(pageSize, limitStage["$limit"].AsInt32);

            // Validar $project final
            BsonDocument projectStage = pipeline[8];
            Assert.IsTrue(projectStage.Contains("$project"));
            Assert.IsTrue(projectStage["$project"].AsBsonDocument.Contains("idRegla"));
            Assert.IsTrue(projectStage["$project"].AsBsonDocument.Contains("nombreRegla"));
            Assert.IsTrue(projectStage["$project"].AsBsonDocument.Contains("casos"));
        }

        [TestMethod]
        public void BuildTotalesPipeline_ConSubprocesoValido_GeneraPipelineBsonConGroupNull()
        {
            // Arrange
            string subProcesoId = "SP-550";

            // Act
            BsonDocument[] pipeline = GetReglasFallidasBySubprocesoBuilder.BuildTotalesPipeline(subProcesoId);

            // Assert
            Assert.IsNotNull(pipeline);
            Assert.AreEqual(6, pipeline.Length); // 5 base + 1 group

            // Validar última etapa $group
            BsonDocument groupStage = pipeline[5];
            Assert.IsTrue(groupStage.Contains("$group"));

            BsonDocument groupDoc = groupStage["$group"].AsBsonDocument;
            Assert.AreEqual(BsonNull.Value, groupDoc["_id"]);
            Assert.IsTrue(groupDoc.Contains("casosTotales"));
        }
    }
}