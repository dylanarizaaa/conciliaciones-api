using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetResumenProyectosBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_AlInvocar_GeneraPipelineBsonConCincoEtapas()
        {
            // Arrange & Act
            BsonDocument[] pipeline = GetResumenProyectosBuilder.BuildAggregationPipeline();

            // Assert
            Assert.IsNotNull(pipeline);
            Assert.AreEqual(5, pipeline.Length);

            // Validar $lookup hacia procesos
            BsonDocument stageProcesos = pipeline[0];
            Assert.IsTrue(stageProcesos.Contains("$lookup"));
            Assert.AreEqual("procesos", stageProcesos["$lookup"]["from"].AsString);
            Assert.AreEqual("idProyecto", stageProcesos["$lookup"]["localField"].AsString);

            // Validar $lookup hacia subprocesos
            BsonDocument stageSubprocesos = pipeline[1];
            Assert.IsTrue(stageSubprocesos.Contains("$lookup"));
            Assert.AreEqual("subprocesos", stageSubprocesos["$lookup"]["from"].AsString);

            // Validar $lookup hacia parametrizacion
            BsonDocument stageParametrizacion = pipeline[2];
            Assert.IsTrue(stageParametrizacion.Contains("$lookup"));
            Assert.AreEqual("parametrizacion", stageParametrizacion["$lookup"]["from"].AsString);

            // Validar $lookup hacia conciliaciones
            BsonDocument stageConciliaciones = pipeline[3];
            Assert.IsTrue(stageConciliaciones.Contains("$lookup"));
            Assert.AreEqual("conciliaciones", stageConciliaciones["$lookup"]["from"].AsString);

            // Validar $project final
            BsonDocument stageProject = pipeline[4];
            Assert.IsTrue(stageProject.Contains("$project"));
            BsonDocument projectDoc = stageProject["$project"].AsBsonDocument;
            Assert.IsTrue(projectDoc.Contains("idProyecto"));
            Assert.IsTrue(projectDoc.Contains("nombreProyecto"));
            Assert.IsTrue(projectDoc.Contains("conciliacionesFallidas"));
        }
    }
}