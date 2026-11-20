using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient;
using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class ApisExternasControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock = new();

        [TestMethod]
        public void Constructor_ConMediatorNulo_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ApisExternasController(null!));
        }

        [TestMethod]
        public async Task GetParametrizaciones_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            GetParameterizationsByClientResponse response = new(
            [
                new RulesParamsResponseDto("param-1", null, [new RuleParamDto("r1", "Regla")])
            ]);
            Answers<GetParameterizationsByClientResponse> expected = new(response)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetParameterizationsByClientQuery>(q =>
                        q.ProyectoId == "proy" && q.ProcesoId == null && q.SubProcesoId == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            ApisExternasController controller = new(_mediatorMock.Object);
            ActionResult<Answers<GetParameterizationsByClientResponse>> actionResult =
                await controller.GetParametrizaciones("proy", null, null, CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
            Assert.AreEqual(expected, objectResult.Value);
        }

        [TestMethod]
        public async Task GetParametrizaciones_BadRequest_Retorna400()
        {
            Answers<GetParameterizationsByClientResponse> expected = new(
                "Debe informar proyectoId, procesoId o subProcesoId.",
                HttpStatusCode.BadRequest);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetParameterizationsByClientQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            ApisExternasController controller = new(_mediatorMock.Object);
            ActionResult<Answers<GetParameterizationsByClientResponse>> actionResult =
                await controller.GetParametrizaciones(null, null, null, CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }
    }
}
