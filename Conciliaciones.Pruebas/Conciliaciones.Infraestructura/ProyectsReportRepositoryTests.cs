using Conciliaciones.Dominio.DTOs.Response.Projects;
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
    public class ProyectsReportRepositoryTests
    {
        [TestMethod]
        public async Task GetResumenProyectosAsync_ConProyectosYConciliacionesFallidas_DevuelveResumenCalculado()
        {
            List<ProyectoResumenProjection> mockList = new List<ProyectoResumenProjection>
            {
                new ProyectoResumenProjection
                {
                    IdProyecto = "PROY-100",
                    NombreProyecto = "Proyecto Recaudos Corp",
                    ConciliacionesFallidas = new List<ConciliacionMontoProjection>
                    {
                        new ConciliacionMontoProjection
                        {
                            ResultadoTransformacion = new Dictionary<string, object>
                            {
                                { "diferenciaCapital", 3500.50m },
                                { "recargoMora", "499.50" }
                            }
                        }
                    }
                }
            };

            ProyectsReportRepository repository = CreateRepository(mockList);
            List<ProjectSummaryDto> resultado = await repository.GetResumenProyectosAsync(CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("PROY-100", resultado[0].ProyectoId);
            Assert.AreEqual("Proyecto Recaudos Corp", resultado[0].NombreProyecto);
            Assert.AreEqual(1, resultado[0].Cantidad);
            Assert.AreEqual(4000.00m, resultado[0].Monto);
        }

        [TestMethod]
        public async Task GetResumenProyectosAsync_SinConciliacionesFallidas_DevuelveMontoYCantidadEnCero()
        {
            List<ProyectoResumenProjection> mockList = new List<ProyectoResumenProjection>
            {
                new ProyectoResumenProjection
                {
                    IdProyecto = "PROY-200",
                    NombreProyecto = "Proyecto Nomina Especial",
                    ConciliacionesFallidas = new List<ConciliacionMontoProjection>()
                }
            };

            ProyectsReportRepository repository = CreateRepository(mockList);
            List<ProjectSummaryDto> resultado = await repository.GetResumenProyectosAsync(CancellationToken.None);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("PROY-200", resultado[0].ProyectoId);
            Assert.AreEqual(0, resultado[0].Cantidad);
            Assert.AreEqual(0m, resultado[0].Monto);
        }

        private static ProyectsReportRepository CreateRepository(List<ProyectoResumenProjection> results)
        {
            Mock<IAsyncCursor<ProyectoResumenProjection>> cursorMock = new Mock<IAsyncCursor<ProyectoResumenProjection>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(results);

            Mock<IMongoCollection<ProyectoDocument>> collectionMock = new Mock<IMongoCollection<ProyectoDocument>>();
            collectionMock.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<ProyectoDocument, ProyectoResumenProjection>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(cursorMock.Object);

            IOptions<DocumentDbSettings> options = Options.Create(new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            });
            Mock<DocumentDBContext> contextMock = new Mock<DocumentDBContext>(options);
            contextMock.Setup(c => c.GetCollection<ProyectoDocument>(It.IsAny<string>())).Returns(collectionMock.Object);
            return new ProyectsReportRepository(
                contextMock.Object,
                new FailedConciliationSummaryMapper(new TransformacionMontoCalculator()));
        }
    }
}
