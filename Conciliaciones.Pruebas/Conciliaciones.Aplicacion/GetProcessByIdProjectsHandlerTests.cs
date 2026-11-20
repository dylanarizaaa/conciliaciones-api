using Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;
using Conciliaciones.Dominio.DTOs.Response.Process;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Process;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects
{
    [TestClass]
    public class GetProcessByIdProjectsHandlerTests
    {
        private readonly Mock<IProcessRepository> _processRepositoryMock;

        public GetProcessByIdProjectsHandlerTests()
        {
            _processRepositoryMock = new Mock<IProcessRepository>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetProcessByIdProjectsHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetProcessByIdProjectsHandler handler = new GetProcessByIdProjectsHandler(_processRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaSumatoriasCorrectasYData()
        {
            // Arrange
            string proyectoId = "6ac65d82e65b7e1cc244a8ba";

            List<ProcesstSummaryDto> listaResumenesMock = new List<ProcesstSummaryDto>
            {
                new ProcesstSummaryDto("PROC-01", "Proceso Facturación Interpay", 95000.50m, 25),
                new ProcesstSummaryDto("PROC-02", "Proceso Recaudos y Pagos", 55250.25m, 17)
            };

            _processRepositoryMock
                .Setup(r => r.GetProcessesByIdProjectsAsync(proyectoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaResumenesMock);

            GetProcessByIdProjectsQuery query = new GetProcessByIdProjectsQuery(proyectoId);
            GetProcessByIdProjectsHandler handler = new GetProcessByIdProjectsHandler(_processRepositoryMock.Object);

            // Act
            Answers<GetProcessByIdProjectsResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(150250.75m, result.Resultado.totalAfectado);
            Assert.AreEqual(42L, result.Resultado.casosTotales);
            Assert.AreEqual(2, result?.Resultado.data.Count);

            _processRepositoryMock.Verify(r => r.GetProcessesByIdProjectsAsync(proyectoId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ListaVacia_RetornaTotalesEnCero()
        {
            // Arrange
            string proyectoId = "6ac65d82e65b7e1cc244a8bb";

            List<ProcesstSummaryDto> listaResumenesVacia = new List<ProcesstSummaryDto>();

            _processRepositoryMock
                .Setup(r => r.GetProcessesByIdProjectsAsync(proyectoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaResumenesVacia);

            GetProcessByIdProjectsQuery query = new GetProcessByIdProjectsQuery(proyectoId);
            GetProcessByIdProjectsHandler handler = new GetProcessByIdProjectsHandler(_processRepositoryMock.Object);

            // Act
            Answers<GetProcessByIdProjectsResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(0m, result.Resultado.totalAfectado);
            Assert.AreEqual(0L, result.Resultado.casosTotales);
            Assert.AreEqual(0, result?.Resultado.data.Count);

            _processRepositoryMock.Verify(r => r.GetProcessesByIdProjectsAsync(proyectoId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}