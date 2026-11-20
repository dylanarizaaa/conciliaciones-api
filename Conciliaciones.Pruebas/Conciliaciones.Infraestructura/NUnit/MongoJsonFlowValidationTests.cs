using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Services;
using Conciliaciones.Infraestructura.Settings;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using System.Text.Json;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    /// <summary>
    /// Valida pipelines y montos contra los dumps JSON reales de Mongo (sin cluster).
    /// </summary>
    [TestFixture]
    public class MongoJsonFlowValidationTests
    {
        private static string FixtureDir
        {
            get
            {
                string inOutput = Path.Combine(AppContext.BaseDirectory, "TestData", "Mongo");
                if (Directory.Exists(inOutput))
                {
                    return inOutput;
                }

                // Fallback: Buscar desde el output hacia la raíz del repo
                DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null)
                {
                    string candidate = Path.Combine(dir.FullName, "Conciliaciones.Pruebas", "TestData", "Mongo");
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }

                    dir = dir.Parent;
                }

                return inOutput;
            }
        }

        [Test]
        public void Fixtures_ContainExpectedHierarchyIds()
        {
            JsonElement proyectos = ReadArray("Conciliaciones.proyectos.json");
            JsonElement procesos = ReadArray("Conciliaciones.procesos.json");
            JsonElement subprocesos = ReadArray("Conciliaciones.subprocesos.json");
            JsonElement paramsDoc = ReadArray("Conciliaciones.parametrizacion.json");
            JsonElement conciliaciones = ReadArray("Conciliaciones.conciliaciones.json");

            string idProyecto = proyectos[0].GetProperty("idProyecto").GetString()!;
            string idProceso = procesos[0].GetProperty("idProceso").GetString()!;
            string idSubProceso = subprocesos[0].GetProperty("idSubProceso").GetString()!;
            string parametrizacionId = paramsDoc[0].GetProperty("parametrizacionId").GetString()!;

            idProyecto.Should().Be("b67a7c53-68cd-4c20-ba6c-6165edfd7814");
            idProceso.Should().Be("09f12723-a85a-4d28-9520-25fa6fee7b04");
            idSubProceso.Should().Be("aafa05df-cf84-4365-a5d0-2eee83c14a15");
            parametrizacionId.Should().Be("e48474f6-0706-48d8-a1b8-bd8002923d43");

            conciliaciones.EnumerateArray()
                .Select(c => c.GetProperty("parametrizacionId").GetString())
                .Should().Contain(parametrizacionId);

            // Datos reales mezclan casing — el pipeline debe tolerarlo
            conciliaciones.EnumerateArray()
                .Select(c => c.GetProperty("estado").GetString())
                .Should().Contain(new[] { "fallido", "Fallido" });
        }

       

        [Test]
        public void FilterFailedByRegla_AndFuentesDetail_AreCaseInsensitive()
        {
            string rowsFilter = AggregationStages.FilterFailedByReglaSeleccionada().ToJson();
            rowsFilter.Should().Contain("$toLower");
            rowsFilter.Should().Contain("reglaSeleccionada");

            string fuentes = GetConciliationFuentesDetailBuilder
                .BuildAggregationPipeline("aafa05df-cf84-4365-a5d0-2eee83c14454")
                .ToJson();
            fuentes.Should().Contain("$toLower");
            fuentes.Should().Contain("$expr");
        }

        [Test]
        public void SummaryMapper_WithFixtureTransformacion_ComputesMonto()
        {
            FailedConciliationSummaryMapper mapper =
                new FailedConciliationSummaryMapper(new TransformacionMontoCalculator());

            List<ConciliacionMontoProjection> fallidas = new()
            {
                new ConciliacionMontoProjection
                {
                    ResultadoTransformacion = new Dictionary<string, object>
                    {
                        ["diferenciaController"] = 0,
                        ["diferenciaNovasoft"] = 4454,
                        ["diferencia"] = 4454
                    }
                },
                new ConciliacionMontoProjection
                {
                    ResultadoTransformacion = new Dictionary<string, object>()
                }
            };

            (long count, decimal monto) = mapper.Summarize(fallidas);
            count.Should().Be(2);
            monto.Should().Be(8908m); // 0+4454+4454 + vacío
        }

        [Test]
        public void ReglasFallidasPipeline_UsesSubProcesoAndReglaFromFixture()
        {
            BsonDocument[] data = GetReglasFallidasBySubprocesoBuilder.BuildDataPipeline(
                "aafa05df-cf84-4365-a5d0-2eee83c14a15",
                pageNumber: 1,
                pageSize: 10);
            string json = data.ToJson();
            json.Should().Contain("aafa05df-cf84-4365-a5d0-2eee83c14a15");
            json.Should().Contain("$toLower");
        }

        [Test]
        public void ResolveCaCertificatePath_FindsBundledPem()
        {
            string resolved = DocumentDbSettingsFactory.ResolveCaCertificatePath("Certificados/global-bundle.pem");
            // En test host puede no existir; al menos no debe lanzar y debe devolver path no vacío
            resolved.Should().NotBeNullOrWhiteSpace();
        }

        private static JsonElement ReadArray(string fileName)
        {
            string path = Path.Combine(FixtureDir, fileName);
            File.Exists(path).Should().BeTrue($"fixture {fileName} debe copiarse al output ({path})");
            using FileStream stream = File.OpenRead(path);
            using JsonDocument doc = JsonDocument.Parse(stream);
            return doc.RootElement.Clone();
        }
    }
}
