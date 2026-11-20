using Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient;
using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.ExternalAPIs;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient
{
    [TestClass]
    public class GetParameterizationsByClientHandlerTests
    {
        private readonly Mock<IExternalAPIsRepository> _repoMock = new();

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetParameterizationsByClientHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            GetParameterizationsByClientHandler handler = new(_repoMock.Object);
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_SinFiltros_RetornaBadRequest()
        {
            GetParameterizationsByClientHandler handler = new(_repoMock.Object);
            Answers<GetParameterizationsByClientResponse> result = await handler.Handle(
                new GetParameterizationsByClientQuery(null, null, null),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.CodigoEstado);
            Assert.IsFalse(result.RespuestaEstado);
            _repoMock.Verify(
                r => r.GetParameterizationsByClientAsync(
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Handle_ConSubProceso_RetornaParametrizaciones()
        {
            List<RulesParamsResponseDto> data =
            [
                new RulesParamsResponseDto(
                    "param-1",
                    [new Dictionary<string, string> { ["nombre"] = "n", ["tipo"] = "t" }],
                    [new RuleParamDto("r1", "ReglaUno")])
            ];

            _repoMock
                .Setup(r => r.GetParameterizationsByClientAsync(null, null, "sp-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);

            GetParameterizationsByClientHandler handler = new(_repoMock.Object);
            Answers<GetParameterizationsByClientResponse> result = await handler.Handle(
                new GetParameterizationsByClientQuery(null, null, "sp-1"),
                CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.OK, result.CodigoEstado);
            Assert.IsNotNull(result.Resultado);
            Assert.HasCount(1, result.Resultado.Parametrizaciones);
            Assert.AreEqual("param-1", result.Resultado.Parametrizaciones[0].idParametrizacion);
        }
    }
}
