using Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion;
using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Automatizaciones;
using Moq;
using System.Net;
using DomResponse = Conciliaciones.Dominio.DTOs.Response.Automatizaciones.CreateAutomatizacionResponse;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion
{
    [TestClass]
    public class CreateAutomatizacionHandlerTests
    {
        private readonly Mock<IAutomatizacionWritePort> _writePortMock = new();

        [TestMethod]
        public void Constructor_ConWritePortNulo_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new CreateAutomatizacionHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConCommandNulo_LanzaArgumentNullException()
        {
            CreateAutomatizacionHandler handler = new(_writePortMock.Object);
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConRequestNulo_LanzaArgumentNullException()
        {
            CreateAutomatizacionHandler handler = new(_writePortMock.Object);
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(new CreateAutomatizacionCommand(null!), CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_CuandoPortNoRetornaData_PropagaStatusYMensaje()
        {
            CreateAutomatizacionRequest request = new() { ParametrizacionId = "p1" };
            _writePortMock
                .Setup(p => p.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpStatusCode.NotFound, (DomResponse?)null, "La parametrización indicada no existe."));

            CreateAutomatizacionHandler handler = new(_writePortMock.Object);
            Answers<CreateAutomatizacionResponse> result = await handler.Handle(
                new CreateAutomatizacionCommand(request),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.NotFound, result.CodigoEstado);
            Assert.IsFalse(result.RespuestaEstado);
            Assert.IsNull(result.Resultado);
            Assert.AreEqual("La parametrización indicada no existe.", result.Mensaje);
        }

        [TestMethod]
        public async Task Handle_CreacionExitosa_MapeaResponse()
        {
            CreateAutomatizacionRequest request = new() { ParametrizacionId = "p1", Estado = "activo" };
            DomResponse data = new("auto-1", "p1", "Automatización creada correctamente.");
            _writePortMock
                .Setup(p => p.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpStatusCode.OK, data, "Automatización creada correctamente."));

            CreateAutomatizacionHandler handler = new(_writePortMock.Object);
            Answers<CreateAutomatizacionResponse> result = await handler.Handle(
                new CreateAutomatizacionCommand(request),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.OK, result.CodigoEstado);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual("auto-1", result.Resultado.AutomatizacionId);
            Assert.AreEqual("p1", result.Resultado.ParametrizacionId);
        }
    }
}
