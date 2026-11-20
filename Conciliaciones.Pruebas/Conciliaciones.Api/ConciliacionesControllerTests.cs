using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull;
using Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;
using Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;
using Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Process;
using Conciliaciones.Dominio.DTOs.Response.Projects;
using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class ConciliacionesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;

        public ConciliacionesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
        }

        [TestMethod]
        public void Constructor_ConMediatorNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new ConciliacionesController(null!));
        }

        [TestMethod]
        public async Task GetResumenProyectos_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            List<ProjectSummaryDto> dummyList = new List<ProjectSummaryDto>();
            GetResumenProyectosResponse dummyData = new GetResumenProyectosResponse(dummyList);
            Answers<GetResumenProyectosResponse> expectedAnswer = new Answers<GetResumenProyectosResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetResumenProyectosQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<GetResumenProyectosResponse>> actionResult = await controller.GetResumenProyectos(CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(actionResult.Result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
            Assert.AreEqual(expectedAnswer, objectResult.Value);
        }

        [TestMethod]
        public async Task GetResumenProcesosByProyecto_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            // Arrange
            string proyectoId = "proj-123";
            decimal totalAfectado = 1000.50m;
            long casosTotales = 50;
            List<ProcesstSummaryDto> dummyList = new List<ProcesstSummaryDto>();
            GetProcessByIdProjectsResponse dummyData = new GetProcessByIdProjectsResponse(totalAfectado, casosTotales, dummyList);
            Answers<GetProcessByIdProjectsResponse> expectedAnswer = new Answers<GetProcessByIdProjectsResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetProcessByIdProjectsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            IActionResult result = await controller.GetResumenProcesosByProyecto(proyectoId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task GetResumenSubProcessByProcessId_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            // Arrange
            string processId = "proc-456";
            decimal totalAfectado = 1000.50m;
            long casosTotales = 50;
            List<SubProcessInfoDto> dummyList = new List<SubProcessInfoDto>();
            GetSubProcessByIdProcessResponse dummyData = new GetSubProcessByIdProcessResponse(totalAfectado, casosTotales, dummyList);
            Answers<GetSubProcessByIdProcessResponse> expectedAnswer = new Answers<GetSubProcessByIdProcessResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSubProcessByIdProcessQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            IActionResult result = await controller.GetResumenSubProcessByProcessId(processId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task GetReglasFallidasBySubproceso_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            // Arrange
            string subProcesoId = "subproc-789";
            long casosTotales = 20;
            decimal montoAfectado = 500.75m;
            int pageNumber = 1;
            int pageSize = 10;
            long totalPages = 2;
            List<FailedRuleDto> dummyList = new List<FailedRuleDto>();
            GetReglasFallidasBySubprocesoResponse dummyData = new GetReglasFallidasBySubprocesoResponse(casosTotales, montoAfectado, pageNumber, pageSize, totalPages, dummyList);
            Answers<GetReglasFallidasBySubprocesoResponse> expectedAnswer = new Answers<GetReglasFallidasBySubprocesoResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetReglasFallidasBySubprocesoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<GetReglasFallidasBySubprocesoResponse>> result = await controller.GetReglasFallidasBySubproceso(subProcesoId, 1, 10, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)result.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task GetConciliationRows_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            // Arrange
            string reglaId = "regla-monto-id";
            long casosTotales = 20;
            decimal montoAfectado = 500.75m;
            long totalCount = 100;
            int pageNumber = 1;
            int pageSize = 10;
            int totalPages = 2;
            List<Dictionary<string, object?>> dummyList = new List<Dictionary<string, object?>>();
            GetConciliationRowsResponse dummyData = new GetConciliationRowsResponse(casosTotales, montoAfectado, totalCount, pageNumber, pageSize, totalPages, dummyList);
            Answers<GetConciliationRowsResponse> expectedAnswer = new Answers<GetConciliationRowsResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetConciliationRowsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<GetConciliationRowsResponse>> result = await controller.GetConciliationRows(reglaId, 1, 10, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)result.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task GetFuentesDetalle_EjecucionExitosa_RetornaStatusCodeCorrecto()
        {
            // Arrange
            string conciliacionId = "concil-001";
            List<FuenteDetalleDto> dummyList = new List<FuenteDetalleDto>();
            GetConciliationFuentesDetailResponse dummyData = new GetConciliationFuentesDetailResponse(dummyList);
            Answers<GetConciliationFuentesDetailResponse> expectedAnswer = new Answers<GetConciliationFuentesDetailResponse>(dummyData)
            {
                CodigoEstado = HttpStatusCode.OK
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetConciliationFuentesDetailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<GetConciliationFuentesDetailResponse>> result = await controller.GetFuentesDetalle(conciliacionId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)result.Result!;
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task ChangeEstadoToPendiente_RequestNulo_RetornaBadRequest()
        {
            // Arrange
            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<UpdateConciliacionEstadoResponse>> result = await controller.ChangeEstadoToPendiente(null!, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            BadRequestObjectResult badRequest = (BadRequestObjectResult)result.Result!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task ChangeEstadoToPendiente_ListaConciliacionIdVacia_RetornaBadRequest()
        {
            // Arrange
            UpdateConciliacionEstadoRequest request = new UpdateConciliacionEstadoRequest(new List<Guid>());

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            // Act
            ActionResult<Answers<UpdateConciliacionEstadoResponse>> result = await controller.ChangeEstadoToPendiente(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            BadRequestObjectResult badRequest = (BadRequestObjectResult)result.Result!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task ChangeEstadoToPendiente_RequestValido_EnviosComandoYRetornaStatusCode()
        {
            // Arrange
            List<Guid> idsList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            UpdateConciliacionEstadoRequest requestDto = new UpdateConciliacionEstadoRequest(
                ConciliacionId: idsList
            );

            UpdateConciliacionEstadoResponse responseData = new UpdateConciliacionEstadoResponse(
                actualizados: idsList,
                noEncontrados: new List<Guid>(),
                totalProcesados: 2
            );

            Answers<UpdateConciliacionEstadoResponse> expectedAnswers = new Answers<UpdateConciliacionEstadoResponse>(responseData);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateConciliacionEstadoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswers);
            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);

            ActionResult<Answers<UpdateConciliacionEstadoResponse>> actionResult =
                await controller.ChangeEstadoToPendiente(requestDto, CancellationToken.None);
            Assert.IsNotNull(actionResult);
            ObjectResult objectResult = (ObjectResult)actionResult.Result!;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);

            Answers<UpdateConciliacionEstadoResponse> responseBody =
                (Answers<UpdateConciliacionEstadoResponse>)objectResult.Value!;

            Assert.IsNotNull(responseBody);
            Assert.AreEqual(2, responseBody.Resultado.actualizados.Count);



        }

        [TestMethod]
        public async Task CreateParametrizacionFull_RequestNulo_RetornaBadRequest()
        {
            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);
            ActionResult<Answers<CreateParametrizacionFullResponse>> result =
                await controller.CreateParametrizacionFull(null!, CancellationToken.None);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task CreateParametrizacionFull_RequestValido_RetornaStatusCode()
        {
            CreateParametrizacionFullResponse data = new(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "ok");
            Answers<CreateParametrizacionFullResponse> expected = new(data) { CodigoEstado = HttpStatusCode.OK };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateParametrizacionFullCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            ConciliacionesController controller = new ConciliacionesController(_mediatorMock.Object);
            ActionResult<Answers<CreateParametrizacionFullResponse>> result =
                await controller.CreateParametrizacionFull(new CreateParametrizacionRequest { proyecto = "p" }, CancellationToken.None);

            ObjectResult objectResult = Assert.IsInstanceOfType<ObjectResult>(result.Result);
            Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);
        }
    }
}