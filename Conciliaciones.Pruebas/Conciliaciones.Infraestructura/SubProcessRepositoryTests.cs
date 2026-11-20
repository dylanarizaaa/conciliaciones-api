using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Persistence.Services;
using Conciliaciones.Infraestructura.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Repositories
{
    [TestClass]
    public class SubProcessRepositoryTests
    {
        [TestMethod]
        public async Task GetReglasFallidasBySubprocesoIdAsync_ConDatosYTotalesValidos_DevuelveSubprocesoReglasResumenDto()
        {
            List<ReglaFallidaProjection> data = new List<ReglaFallidaProjection>
            {
                new ReglaFallidaProjection
                {
                    NombreRegla = "ValidacionMontoExacto",
                    Casos = 2L,
                    ConciliacionesFiltradas = new List<ConciliacionMontoProjection>
                    {
                        new ConciliacionMontoProjection
                        {
                            ResultadoTransformacion = new Dictionary<string, object> { { "diferencia", 1200.00m } }
                        },
                        new ConciliacionMontoProjection
                        {
                            ResultadoTransformacion = new Dictionary<string, object> { { "diferencia", 800.00m } }
                        }
                    }
                }
            };
            List<ReglasFallidasTotalesProjection> totales = new List<ReglasFallidasTotalesProjection>
            {
                new ReglasFallidasTotalesProjection { CasosTotales = 2L }
            };

            SubProcessRepository repository = CreateRepositoryForReglas(data, totales);
            SubprocesoReglasResumenDto resultado = await repository.GetReglasFallidasBySubprocesoIdAsync(
                "SP-999", 1, 10, CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(2L, resultado.CasosTotales);
            Assert.AreEqual(2000.00m, resultado.MontoAfectado);
            Assert.AreEqual(1, resultado.Data.Count);
            Assert.AreEqual("ValidacionMontoExacto", resultado.Data[0].nombreRegla);
            Assert.AreEqual(2L, resultado.Data[0].Casos);
            Assert.AreEqual(2000.00m, resultado.Data[0].Monto);
        }

        [TestMethod]
        public async Task GetSubProcesosByIdProcess_ConIdProcesoValido_DevuelveListaSubProcessInfoDto()
        {
            List<SubProcesoResumenProjection> mockList = new List<SubProcesoResumenProjection>
            {
                new SubProcesoResumenProjection
                {
                    IdSubProceso = "SP-888-1",
                    NombreSubProceso = "Subproceso Validaciones Bancarias",
                    ConciliacionesFallidas = new List<ConciliacionMontoProjection>
                    {
                        new ConciliacionMontoProjection
                        {
                            ResultadoTransformacion = new Dictionary<string, object> { { "comision", 350.50m } }
                        }
                    }
                }
            };

            Mock<IAsyncCursor<SubProcesoResumenProjection>> cursorMock = new Mock<IAsyncCursor<SubProcesoResumenProjection>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(mockList);

            Mock<IMongoCollection<SubProcesoDocument>> collectionMock = new Mock<IMongoCollection<SubProcesoDocument>>();
            collectionMock.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<SubProcesoDocument, SubProcesoResumenProjection>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(cursorMock.Object);

            Mock<DocumentDBContext> contextMock = CreateContext();
            contextMock.Setup(c => c.GetCollection<SubProcesoDocument>(It.IsAny<string>())).Returns(collectionMock.Object);
            SubProcessRepository repository = CreateRepository(contextMock);

            List<SubProcessInfoDto> resultado = await repository.GetSubProcesosByIdProcess("PROC-888", CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("SP-888-1", resultado[0].SubProcesoId);
            Assert.AreEqual("Subproceso Validaciones Bancarias", resultado[0].NombreSubProceso);
            Assert.AreEqual(1L, resultado[0].Cantidad);
            Assert.AreEqual(350.50m, resultado[0].Monto);
        }

        private static SubProcessRepository CreateRepositoryForReglas(
            List<ReglaFallidaProjection> data,
            List<ReglasFallidasTotalesProjection> totales)
        {
            Mock<IAsyncCursor<ReglaFallidaProjection>> dataCursor = new Mock<IAsyncCursor<ReglaFallidaProjection>>();
            dataCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            dataCursor.Setup(c => c.Current).Returns(data);

            Mock<IAsyncCursor<ReglasFallidasTotalesProjection>> totalesCursor = new Mock<IAsyncCursor<ReglasFallidasTotalesProjection>>();
            totalesCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            totalesCursor.Setup(c => c.Current).Returns(totales);

            Mock<IMongoCollection<ParametrizacionDocument>> collectionMock = new Mock<IMongoCollection<ParametrizacionDocument>>();
            collectionMock.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<ParametrizacionDocument, ReglaFallidaProjection>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(dataCursor.Object);
            collectionMock.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<ParametrizacionDocument, ReglasFallidasTotalesProjection>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(totalesCursor.Object);

            Mock<DocumentDBContext> contextMock = CreateContext();
            contextMock.Setup(c => c.GetCollection<ParametrizacionDocument>(It.IsAny<string>())).Returns(collectionMock.Object);
            return CreateRepository(contextMock);
        }

        private static SubProcessRepository CreateRepository(Mock<DocumentDBContext> contextMock) =>
            new SubProcessRepository(
                contextMock.Object,
                new FailedConciliationSummaryMapper(new TransformacionMontoCalculator()));

        private static Mock<DocumentDBContext> CreateContext()
        {
            IOptions<DocumentDbSettings> options = Options.Create(new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            });
            return new Mock<DocumentDBContext>(options);
        }
    }
}
