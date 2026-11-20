using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla
{
    [TestClass]
    public class GetParametrizacionPlantillaHandlerTests
    {
        private readonly Mock<IParametrizacionPlantillaQueryPort> _port = new();

        [TestMethod]
        public async Task Handle_TipoInvalido_RetornaBadRequest()
        {
            GetParametrizacionPlantillaHandler handler = new(_port.Object);
            Answers<GetParametrizacionPlantillaResponse> result = await handler.Handle(
                new GetParametrizacionPlantillaQuery("regla", "otro"),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.CodigoEstado);
        }

        [TestMethod]
        public async Task Handle_TableExistente_RetornaPlantilla()
        {
            _port.Setup(p => p.GetPlantillaJsonAsync("regla", "table", It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\"layout\":{\"type\":\"stack\"}}");

            GetParametrizacionPlantillaHandler handler = new(_port.Object);
            Answers<GetParametrizacionPlantillaResponse> result = await handler.Handle(
                new GetParametrizacionPlantillaQuery("regla", "table"),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.OK, result.CodigoEstado);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual("stack", result.Resultado.Plantilla.GetProperty("layout").GetProperty("type").GetString());
        }

        [TestMethod]
        public async Task Handle_SinPlantilla_RetornaNotFound()
        {
            _port.Setup(p => p.GetPlantillaJsonAsync("regla", "accordion", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            GetParametrizacionPlantillaHandler handler = new(_port.Object);
            Answers<GetParametrizacionPlantillaResponse> result = await handler.Handle(
                new GetParametrizacionPlantillaQuery("regla", "accordion"),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.NotFound, result.CodigoEstado);
        }
    }
}
