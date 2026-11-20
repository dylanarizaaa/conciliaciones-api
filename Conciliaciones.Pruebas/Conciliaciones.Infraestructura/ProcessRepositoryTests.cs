using Conciliaciones.Dominio.DTOs.Response.Process;
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
    public class ProcessRepositoryTests
    {
        [TestMethod]
        public async Task GetProcessesByIdProjectsAsync_ConProyectoValidoYConciliacionesFallidas_DevuelveListaDeProcesosCalculada()
        {
            string proyectoId = "PROY-001";
            List<ProcesoResumenProjection> mockList = new List<ProcesoResumenProjection>
            {
                new ProcesoResumenProjection
                {
                    IdProceso = "PROC-101",
                    NombreProceso = "Proceso Pagos Recibidos",
                    ConciliacionesFallidas = new List<ConciliacionMontoProjection>
                    {
                        new ConciliacionMontoProjection
                        {
                            ResultadoTransformacion = new Dictionary<string, object>
                            {
                                { "montoDiferencia", 1250.75m },
                                { "interesMulta", "249.25" }
                            }
                        }
                    }
                }
            };

            ProcessRepository repository = CreateRepository(mockList);
            List<ProcesstSummaryDto> resultado = await repository.GetProcessesByIdProjectsAsync(proyectoId, CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("PROC-101", resultado[0].ProcesoId);
            Assert.AreEqual("Proceso Pagos Recibidos", resultado[0].NombreProceso);
            Assert.AreEqual(1, resultado[0].Cantidad);
            Assert.AreEqual(1500.00m, resultado[0].Monto);
        }

        [TestMethod]
        public async Task GetProcessesByIdProjectsAsync_SinConciliacionesFallidas_DevuelveMontoYCantidadEnCero()
        {
            List<ProcesoResumenProjection> mockList = new List<ProcesoResumenProjection>
            {
                new ProcesoResumenProjection
                {
                    IdProceso = "PROC-202",
                    NombreProceso = "Proceso Facturación Directa",
                    ConciliacionesFallidas = new List<ConciliacionMontoProjection>()
                }
            };

            ProcessRepository repository = CreateRepository(mockList);
            List<ProcesstSummaryDto> resultado = await repository.GetProcessesByIdProjectsAsync("PROY-002", CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("PROC-202", resultado[0].ProcesoId);
            Assert.AreEqual(0, resultado[0].Cantidad);
            Assert.AreEqual(0m, resultado[0].Monto);
        }

        private static ProcessRepository CreateRepository(List<ProcesoResumenProjection> results)
        {
            Mock<IAsyncCursor<ProcesoResumenProjection>> cursorMock = new Mock<IAsyncCursor<ProcesoResumenProjection>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(results);

            Mock<IMongoCollection<ProcesoDocument>> collectionMock = new Mock<IMongoCollection<ProcesoDocument>>();
            collectionMock.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<ProcesoDocument, ProcesoResumenProjection>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(cursorMock.Object);

            Mock<DocumentDBContext> contextMock = CreateContext(collectionMock.Object);
            return new ProcessRepository(
                contextMock.Object,
                new FailedConciliationSummaryMapper(new TransformacionMontoCalculator()));
        }

        private static Mock<DocumentDBContext> CreateContext(IMongoCollection<ProcesoDocument> collection)
        {
            IOptions<DocumentDbSettings> options = Options.Create(new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            });
            Mock<DocumentDBContext> contextMock = new Mock<DocumentDBContext>(options);
            contextMock.Setup(c => c.GetCollection<ProcesoDocument>(It.IsAny<string>())).Returns(collection);
            return contextMock;
        }
    }
}
