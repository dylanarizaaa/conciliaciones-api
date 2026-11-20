using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class InformesControlerTests
    {
        private readonly Mock<IMediator> _mediator = new();

        [TestMethod]
        public void Constructor_NullMediator_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new InformesController(null!));
        }

        [TestMethod]
        public async Task ExportExcel_DelegatesToMediator_AndReturnsStatusCode()
        {
            ExcelDataReportsResponse payload = new("YmFzZTY0", "Reporte.xlsx", "application/octet-stream");
            Answers<ExcelDataReportsResponse> answer = new(payload) { CodigoEstado = HttpStatusCode.OK };

            _mediator
                .Setup(m => m.Send(
                    It.Is<ExcelDataReportsQuery>(q => q.ReglaId == "regla-id-1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(answer);

            InformesController controller = new(_mediator.Object);
            ActionResult<Answers<ExcelDataReportsResponse>> actionResult =
                await controller.ExportExcel("regla-id-1", CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
            Assert.AreEqual(answer, objectResult.Value);
        }

        [TestMethod]
        public async Task ExportExcel_NotFound_PropagatesStatus()
        {
            Answers<ExcelDataReportsResponse> answer =
                new("No hay conciliaciones", HttpStatusCode.NotFound);

            _mediator
                .Setup(m => m.Send(It.IsAny<ExcelDataReportsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(answer);

            InformesController controller = new(_mediator.Object);
            ActionResult<Answers<ExcelDataReportsResponse>> actionResult =
                await controller.ExportExcel("regla", CancellationToken.None);

            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }
    }
}
