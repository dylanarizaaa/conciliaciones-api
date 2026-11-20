using Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;
using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado
{
    [TestClass]
    public class UpdateConciliacionEstadoHandlerTests
    {
        private readonly Mock<IConciliationStateService> _conciliationRepositoryMock;

        public UpdateConciliacionEstadoHandlerTests()
        {
            _conciliationRepositoryMock = new Mock<IConciliationStateService>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new UpdateConciliacionEstadoHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            UpdateConciliacionEstadoHandler handler = new UpdateConciliacionEstadoHandler(_conciliationRepositoryMock.Object);
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_EjecucionExitosa_RetornaAnswersConResponseCorrecto()
        {
            List<Guid> idsIngresados = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            List<Guid> idsActualizados = new List<Guid> { idsIngresados[0] };
            List<Guid> idsNoEncontrados = new List<Guid> { idsIngresados[1] };
            int totalProcesados = 2;

            StatusUpdateConciliation statusResult = new StatusUpdateConciliation
            (
                idsActualizados,
                idsNoEncontrados,
                totalProcesados
            );

            _conciliationRepositoryMock
                .Setup(r => r.UpdateEstadoAsync(idsIngresados, It.IsAny<CancellationToken>()))
                .ReturnsAsync(statusResult);

            UpdateConciliacionEstadoCommand command = new UpdateConciliacionEstadoCommand(idsIngresados);
            UpdateConciliacionEstadoHandler handler = new UpdateConciliacionEstadoHandler(_conciliationRepositoryMock.Object);

            Answers<UpdateConciliacionEstadoResponse> result = await handler.Handle(command, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(1, result?.Resultado.actualizados.Count);
            Assert.AreEqual(1, result?.Resultado.noEncontrados.Count);
            Assert.AreEqual(totalProcesados, result?.Resultado.totalProcesados);

            _conciliationRepositoryMock.Verify(r => r.UpdateEstadoAsync(idsIngresados, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
