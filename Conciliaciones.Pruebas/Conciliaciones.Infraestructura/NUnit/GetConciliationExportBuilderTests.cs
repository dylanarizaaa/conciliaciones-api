using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class GetConciliationExportBuilderTests
    {
        [Test]
        public void BuildAggregationPipeline_MatchesReglaId_AndHasNoSlice()
        {
            string reglaId = "d75f5195-9109-4aea-a617-9c114dbc0443";

            BsonDocument[] pipeline = GetConciliationExportBuilder.BuildAggregationPipeline(reglaId);

            pipeline.Should().NotBeEmpty();
            string json = string.Join("|", pipeline.Select(p => p.ToJson()));

            json.Should().NotContain("subProcesoId");
            json.Should().Contain("reglas.idRegla");
            json.Should().Contain(reglaId);
            json.Should().Contain("reglaSeleccionada");
            json.Should().NotContain("$slice");
        }
    }
}
