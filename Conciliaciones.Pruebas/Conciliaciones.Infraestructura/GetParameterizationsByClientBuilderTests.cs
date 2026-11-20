using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    [TestClass]
    public class GetParameterizationsByClientBuilderTests
    {
        [TestMethod]
        public void BuildAggregationPipeline_SoloSubProceso_MatchYProject()
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                null, null, "sp-1");

            Assert.HasCount(2, pipeline);
            Assert.IsTrue(pipeline[0].Contains("$match"));
            Assert.AreEqual("sp-1", pipeline[0]["$match"]["subProcesoId"].AsString);
            Assert.IsTrue(pipeline[1].Contains("$project"));
            BsonDocument project = pipeline[1]["$project"].AsBsonDocument;
            Assert.IsTrue(project.Contains("parametrizacionId"));
            Assert.IsTrue(project.Contains("parametros"));
            Assert.IsTrue(project.Contains("reglas"));
        }

        [TestMethod]
        public void BuildAggregationPipeline_SubProcesoConProcesoYProyecto_IncluyeLookups()
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                "proy-1", "proc-1", "sp-1");

            Assert.IsGreaterThanOrEqualTo(6, pipeline.Length);
            Assert.AreEqual("sp-1", pipeline[0]["$match"]["subProcesoId"].AsString);
            Assert.IsTrue(pipeline.Any(s => s.Contains("$lookup") && s["$lookup"]["from"] == "subprocesos"));
            Assert.IsTrue(pipeline.Any(s => s.Contains("$lookup") && s["$lookup"]["from"] == "procesos"));
            Assert.IsTrue(pipeline[^1].Contains("$project"));
        }

        [TestMethod]
        public void BuildAggregationPipeline_SoloProceso_ArrancaEnSubProcesos()
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                null, "proc-1", null);

            Assert.AreEqual("proc-1", pipeline[0]["$match"]["idProceso"].AsString);
            Assert.IsTrue(pipeline.Any(s => s.Contains("$lookup") && s["$lookup"]["from"] == "parametrizacion"));
            Assert.IsTrue(pipeline[^1].Contains("$project"));
            Assert.Contains(
                "parametrizaciones",
                pipeline[^1]["$project"].AsBsonDocument["parametrizacionId"].AsString);
        }

        [TestMethod]
        public void BuildAggregationPipeline_ProcesoConProyecto_ValidaPertenencia()
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                "proy-1", "proc-1", null);

            Assert.IsTrue(pipeline.Any(s =>
                s.Contains("$match")
                && s["$match"].AsBsonDocument.Contains("procesoDoc.idProyecto")));
            Assert.IsTrue(pipeline[^1].Contains("$project"));
        }

        [TestMethod]
        public void BuildAggregationPipeline_SoloProyecto_LookupSubProcesosYParametrizacion()
        {
            BsonDocument[] pipeline = GetParameterizationsByClientBuilder.BuildAggregationPipeline(
                "proy-1", null, null);

            Assert.AreEqual("proy-1", pipeline[0]["$match"]["idProyecto"].AsString);
            Assert.IsTrue(pipeline.Any(s => s.Contains("$lookup") && s["$lookup"]["from"] == "subprocesos"));
            Assert.IsTrue(pipeline.Any(s => s.Contains("$lookup") && s["$lookup"]["from"] == "parametrizacion"));
            Assert.IsTrue(pipeline[^1].Contains("$project"));
        }
    }
}
