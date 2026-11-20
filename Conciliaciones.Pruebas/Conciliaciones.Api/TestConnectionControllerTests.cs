using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Controllers.v1
{
    [TestClass]
    public class TestConnectionControllerTests
    {
        private readonly Mock<IDatabaseProbe> _probeMock = new();
        private readonly Mock<IHostEnvironment> _envMock = new();

        public TestConnectionControllerTests()
        {
            _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);
        }

        [TestMethod]
        public void Constructor_ConDependenciasNulas_LanzaArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new TestConnectionController(null!, _envMock.Object));
            Assert.ThrowsExactly<ArgumentNullException>(() => new TestConnectionController(_probeMock.Object, null!));
        }

        [TestMethod]
        public async Task CheckMongoConnection_FueraDeDevelopment_RetornaNotFound()
        {
            _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Production);
            TestConnectionController controller = new TestConnectionController(_probeMock.Object, _envMock.Object);

            IActionResult resultado = await controller.CheckMongoConnection(CancellationToken.None);

            Assert.IsInstanceOfType(resultado, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CheckDatabases_FueraDeDevelopment_RetornaNotFound()
        {
            _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Production);
            TestConnectionController controller = new TestConnectionController(_probeMock.Object, _envMock.Object);

            IActionResult resultado = await controller.CheckDatabases(CancellationToken.None);

            Assert.IsInstanceOfType(resultado, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CheckMongoConnection_ErrorExcepcion_RetornaStatusCode500()
        {
            _probeMock
                .Setup(p => p.PingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabasePingResult(false, "Error de conexión con MongoDB", string.Empty, string.Empty, Array.Empty<string>(), "Error de conexion simulado", null));

            TestConnectionController controller = new TestConnectionController(_probeMock.Object, _envMock.Object);
            IActionResult resultado = await controller.CheckMongoConnection(CancellationToken.None);

            ObjectResult objectResult = Assert.IsInstanceOfType<ObjectResult>(resultado);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task CheckDatabases_ErrorExcepcion_RetornaStatusCode500()
        {
            _probeMock
                .Setup(p => p.ListDatabasesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabaseCatalogResult(false, "Error al listar las bases de datos (Posible falta de permisos del usuario)", Array.Empty<string>(), string.Empty, "Sin permisos de administrador", null));

            TestConnectionController controller = new TestConnectionController(_probeMock.Object, _envMock.Object);
            IActionResult resultado = await controller.CheckDatabases(CancellationToken.None);

            ObjectResult objectResult = Assert.IsInstanceOfType<ObjectResult>(resultado);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task CheckMongoConnection_DevelopmentOk_RetornaOk()
        {
            _probeMock.Setup(p => p.PingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabasePingResult(true, "ok", "{}", "db", new List<string> { "a" }, null, null));
            TestConnectionController controller = new(_probeMock.Object, _envMock.Object);
            IActionResult resultado = await controller.CheckMongoConnection(CancellationToken.None);
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task CheckDatabases_DevelopmentOk_RetornaOk()
        {
            _probeMock.Setup(p => p.ListDatabasesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabaseCatalogResult(true, "OK", new List<string> { "db" }, "db", null, null));
            TestConnectionController controller = new(_probeMock.Object, _envMock.Object);
            IActionResult resultado = await controller.CheckDatabases(CancellationToken.None);
            Assert.IsInstanceOfType(resultado, typeof(OkObjectResult));
        }
    }
}
