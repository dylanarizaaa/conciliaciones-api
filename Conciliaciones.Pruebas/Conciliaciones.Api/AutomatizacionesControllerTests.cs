using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion;
using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class AutomatizacionesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock = new();

        [TestMethod]
        public void Constructor_ConMediatorNulo_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new AutomatizacionesController(null!));
        }

        [TestMethod]
        public async Task Create_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            CreateAutomatizacionRequest request = new() { ParametrizacionId = "p1" };
            CreateAutomatizacionResponse response = new("auto-1", "p1", "ok");
            Answers<CreateAutomatizacionResponse> expected = new(response) { CodigoEstado = HttpStatusCode.OK };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<CreateAutomatizacionCommand>(c => c.Request == request),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            AutomatizacionesController controller = new(_mediatorMock.Object);
            ActionResult<Answers<CreateAutomatizacionResponse>> actionResult =
                await controller.Create(request, CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
            Assert.AreEqual(expected, objectResult.Value);
        }

        [TestMethod]
        public async Task Create_Conflict_Retorna409()
        {
            CreateAutomatizacionRequest request = new() { ParametrizacionId = "p1" };
            Answers<CreateAutomatizacionResponse> expected = new(
                "Ya existe una automatización para esa parametrización.",
                HttpStatusCode.Conflict);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateAutomatizacionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            AutomatizacionesController controller = new(_mediatorMock.Object);
            ActionResult<Answers<CreateAutomatizacionResponse>> actionResult =
                await controller.Create(request, CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.Conflict, objectResult.StatusCode);
        }
    }
}
