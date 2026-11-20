using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;
using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess
{
    [TestClass]
    public class GetSubProcessByIdProcessHandlerTests
    {
        private readonly Mock<ISubProcessRepository> _subProcessRepositoryMock;

        public GetSubProcessByIdProcessHandlerTests()
        {
            _subProcessRepositoryMock = new Mock<ISubProcessRepository>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetSubProcessByIdProcessHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetSubProcessByIdProcessHandler handler = new GetSubProcessByIdProcessHandler(_subProcessRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaSumatoriasCorrectasYData()
        {
            // Arrange
            string idProcess = "PROC-101";

            List<SubProcessInfoDto> listaResumenesMock = new List<SubProcessInfoDto>
            {
                new SubProcessInfoDto("SP-01", "Subproceso Facturación Directa", 120000.00m, 15),
                new SubProcessInfoDto("SP-02", "Subproceso Recaudos Electrónicos", 80000.50m, 10)
            };

            _subProcessRepositoryMock
                .Setup(r => r.GetSubProcesosByIdProcess(idProcess, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaResumenesMock);

            GetSubProcessByIdProcessQuery query = new GetSubProcessByIdProcessQuery(idProcess);
            GetSubProcessByIdProcessHandler handler = new GetSubProcessByIdProcessHandler(_subProcessRepositoryMock.Object);

            // Act
            Answers<GetSubProcessByIdProcessResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(200000.50m, result.Resultado.totalAfectado);
            Assert.AreEqual(25L, result.Resultado.casosTotales);
            Assert.AreEqual(2, result?.Resultado.data.Count);
            Assert.AreEqual("SP-01", result?.Resultado.data[0].SubProcesoId);

            _subProcessRepositoryMock.Verify(r => r.GetSubProcesosByIdProcess(idProcess, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ListaVacia_RetornaTotalesEnCero()
        {
            // Arrange
            string idProcess = "PROC-102";

            List<SubProcessInfoDto> listaResumenesVacia = new List<SubProcessInfoDto>();

            _subProcessRepositoryMock
                .Setup(r => r.GetSubProcesosByIdProcess(idProcess, It.IsAny<CancellationToken>()))
                .ReturnsAsync(listaResumenesVacia);

            GetSubProcessByIdProcessQuery query = new GetSubProcessByIdProcessQuery(idProcess);
            GetSubProcessByIdProcessHandler handler = new GetSubProcessByIdProcessHandler(_subProcessRepositoryMock.Object);

            // Act
            Answers<GetSubProcessByIdProcessResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(0m, result.Resultado.totalAfectado);
            Assert.AreEqual(0L, result.Resultado.casosTotales);
            Assert.AreEqual(0, result?.Resultado.data.Count);

            _subProcessRepositoryMock.Verify(r => r.GetSubProcesosByIdProcess(idProcess, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}