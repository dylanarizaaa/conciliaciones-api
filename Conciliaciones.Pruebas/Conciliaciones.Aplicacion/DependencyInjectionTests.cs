using Conciliaciones.Aplicacion;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion
{
    [TestClass]
    public class DependencyInjectionTests
    {
        private ServiceCollection _services = null!;
        private Mock<IConfiguration> _configurationMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _services = new ServiceCollection();
            _configurationMock = new Mock<IConfiguration>();

            // Registrar servicios de Logging requeridos por MediatR
            _services.AddLogging();
        }

        [TestMethod]
        public void AddApplicationLayer_ConServicesNulo_LanzaArgumentNullException()
        {
            // Arrange
            IServiceCollection servicesNull = null!;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                servicesNull.AddApplicationLayer(_configurationMock.Object));
        }

        [TestMethod]
        public void AddApplicationLayer_ConfiguracionValida_RegistraServiciosDeMediatR()
        {
            // Arrange & Act
            _services.AddApplicationLayer(_configurationMock.Object);
            ServiceProvider serviceProvider = _services.BuildServiceProvider();

            // Assert
            IMediator mediator = serviceProvider.GetService<IMediator>()!;
            Assert.IsNotNull(mediator, "El servicio IMediator no fue registrado correctamente.");
            Assert.IsInstanceOfType(mediator, typeof(IMediator));
        }

        [TestMethod]
        public void AddApplicationLayer_RegistraSuficientesDescriptoresEnContenedor()
        {
            // Arrange
            int conteoInicial = _services.Count;

            // Act
            _services.AddApplicationLayer(_configurationMock.Object);

            // Assert
            Assert.IsGreaterThan(conteoInicial, _services.Count, "No se registraron descriptores de servicio en la colección.");
        }
    }
}