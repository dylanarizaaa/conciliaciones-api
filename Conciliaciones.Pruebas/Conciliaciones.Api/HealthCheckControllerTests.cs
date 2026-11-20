using Conciliaciones.Api.Controllers.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class HealthCheckControllerTests
    {
        private readonly Mock<HealthCheckService> _healthCheckServiceMock;
        private readonly Mock<ILogger<HealthCheckController>> _loggerMock;

        public HealthCheckControllerTests()
        {
            _healthCheckServiceMock = new Mock<HealthCheckService>();
            _loggerMock = new Mock<ILogger<HealthCheckController>>();
        }

        [TestMethod]
        public void Constructor_ConParametrosNulos_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new HealthCheckController(null!, _loggerMock.Object));

            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new HealthCheckController(_healthCheckServiceMock.Object, null!));
        }

        [TestMethod]
        public void HealthCheck_EjecucionCorrecta_RetornaOkConMensaje()
        {
            // Arrange
            HealthCheckController controller = new HealthCheckController(
                _healthCheckServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            IActionResult resultado = controller.HealthCheck();

            // Assert
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)resultado;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual("Funcionando.", okResult.Value);
        }

        [TestMethod]
        public void Live_EjecucionCorrecta_RetornaOkConMensajeAlive()
        {
            // Arrange
            HealthCheckController controller = new HealthCheckController(
                _healthCheckServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            IActionResult resultado = controller.Live();

            // Assert
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)resultado;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual("Alive", okResult.Value);
        }

        [TestMethod]
        public async Task Ready_EstadoHealthy_RetornaOkResult()
        {
            // Arrange
            Dictionary<string, HealthReportEntry> entries = new Dictionary<string, HealthReportEntry>
            {
                ["documentdb"] = new HealthReportEntry(
                    HealthStatus.Healthy,
                    "DocumentDB ping OK",
                    TimeSpan.Zero,
                    exception: null,
                    data: null)
            };
            HealthReport reportHealthy = new HealthReport(entries, HealthStatus.Healthy, TimeSpan.Zero);

            _healthCheckServiceMock
                .Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reportHealthy);

            HealthCheckController controller = new HealthCheckController(
                _healthCheckServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            IActionResult resultado = await controller.Ready();

            // Assert
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)resultado;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            string serialized = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("documentdb", serialized);
        }

        [TestMethod]
        public async Task Ready_EstadoUnhealthy_RetornaStatusCode503()
        {
            // Arrange
            Dictionary<string, HealthReportEntry> entries = new Dictionary<string, HealthReportEntry>
            {
                ["documentdb"] = new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    "down",
                    TimeSpan.Zero,
                    exception: null,
                    data: null)
            };
            HealthReport reportUnhealthy = new HealthReport(entries, HealthStatus.Unhealthy, TimeSpan.Zero);

            _healthCheckServiceMock
                .Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reportUnhealthy);

            HealthCheckController controller = new HealthCheckController(
                _healthCheckServiceMock.Object,
                _loggerMock.Object
            );

            // Act
            IActionResult resultado = await controller.Ready();

            // Assert
            Assert.IsInstanceOfType(resultado, typeof(ObjectResult));
            ObjectResult objectResult = (ObjectResult)resultado;
            Assert.AreEqual(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
            string serialized = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
            Assert.Contains("documentdb", serialized);
        }
    }
}