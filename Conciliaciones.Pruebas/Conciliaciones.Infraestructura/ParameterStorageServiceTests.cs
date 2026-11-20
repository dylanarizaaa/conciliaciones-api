using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Conciliaciones.Infraestructura.Settings.AmazonWebServices;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Settings.AmazonWebServices
{
    [TestClass]
    public class ParameterStorageServiceTests
    {
        private Mock<IAmazonSimpleSystemsManagement> _ssmClientMock = null!;
        private IMemoryCache _cache = null!;
        private ParameterStorageService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _ssmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new ParameterStorageService(_ssmClientMock.Object, _cache);
        }

        [TestMethod]
        public async Task GetValueParameterStore_ConNombreParametroValido_DevuelveValorDelParametro()
        {
            // Arrange
            string nombreParametro = "ConexionMotorConciliaciones";
            string valorEsperado = "mongodb://admindocdb:Password123@docdb-cluster.amazonaws.com:27017";

            GetParameterResponse responseMock = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = nombreParametro,
                    Value = valorEsperado
                }
            };

            _ssmClientMock.Setup(s => s.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == nombreParametro && r.WithDecryption == true),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(responseMock);

            // Act
            string resultado = await _service.GetValueParameterStore(nombreParametro);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(valorEsperado, resultado);
            _ssmClientMock.Verify(s => s.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetValueParameterStore_NombreParametroNuloOVacio_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _service.GetValueParameterStore(null!));
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _service.GetValueParameterStore(string.Empty));
        }

        [TestMethod]
        public async Task GetValueParameterStore_CuandoSSMLanzaExcepcion_LanzaExceptionConMensajeDetallado()
        {
            // Arrange
            string nombreParametro = "ParametroInexistente";

            _ssmClientMock.Setup(s => s.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new ParameterNotFoundException("Parameter not found"));

            // Act & Assert
            Exception ex = await Assert.ThrowsExactlyAsync<Exception>(() => _service.GetValueParameterStore(nombreParametro));
            Assert.Contains($"Error al obtener el valor del parámetro '{nombreParametro}' desde Parameter Store", ex.Message);
        }
    }
}