using Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos;
using Conciliaciones.Dominio.DTOs.Response.Projects;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Proyects;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos
{
    [TestClass]
    public class GetResumenProyectosHandlerTests
    {
        private readonly Mock<IProyectsReportRepository> _proyectoReportRepositoryMock;

        public GetResumenProyectosHandlerTests()
        {
            _proyectoReportRepositoryMock = new Mock<IProyectsReportRepository>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetResumenProyectosHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetResumenProyectosHandler handler = new GetResumenProyectosHandler(_proyectoReportRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaAnswersConListaDeProyectos()
        {
            // Arrange
            List<ProjectSummaryDto> listaResumenesMock = new List<ProjectSummaryDto>
            {
                new ProjectSummaryDto("6ac65d82e65b7e1cc244a8ba", "Proyecto Interpay", 150250.75m, 42),
                new ProjectSummaryDto("6ac65d82e65b7e1cc244a8bb", "Proyecto Novasoft", 89000.00m, 18)
            };

            _proyectoReportRepositoryMock
                .Setup(r => r.GetResumenProyectosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaResumenesMock);

            GetResumenProyectosQuery query = new GetResumenProyectosQuery();
            GetResumenProyectosHandler handler = new GetResumenProyectosHandler(_proyectoReportRepositoryMock.Object);

            // Act
            Answers<GetResumenProyectosResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.IsNotNull(result.Resultado.data);
            Assert.AreEqual(2, result?.Resultado.data.Count);
            Assert.AreEqual("Proyecto Interpay", result?.Resultado.data[0].NombreProyecto);
            Assert.AreEqual(150250.75m, result?.Resultado.data[0].Monto);

            _proyectoReportRepositoryMock.Verify(r => r.GetResumenProyectosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ListaVacia_RetornaRespuestaConListaVacia()
        {
            // Arrange
            List<ProjectSummaryDto> listaVaciaMock = new List<ProjectSummaryDto>();

            _proyectoReportRepositoryMock
                .Setup(r => r.GetResumenProyectosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaVaciaMock);

            GetResumenProyectosQuery query = new GetResumenProyectosQuery();
            GetResumenProyectosHandler handler = new GetResumenProyectosHandler(_proyectoReportRepositoryMock.Object);

            // Act
            Answers<GetResumenProyectosResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.IsNotNull(result.Resultado.data);
            Assert.AreEqual(0, result?.Resultado.data.Count);

            _proyectoReportRepositoryMock.Verify(r => r.GetResumenProyectosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}