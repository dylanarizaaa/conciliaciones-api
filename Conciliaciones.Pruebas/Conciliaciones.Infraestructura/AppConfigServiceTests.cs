using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Conciliaciones.Infraestructura.Ports.Interfaces.AWS;
using Conciliaciones.Infraestructura.Settings.AmazonWebServices;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Text;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Settings.AmazonWebServices
{
    [TestClass]
    public class AppConfigServiceTests
    {
        private Mock<IAmazonAppConfigData> _clientMock = null!;
        private Mock<IParameterStoreService> _parameterStoreMock = null!;
        private IMemoryCache _cache = null!;
        private AppConfigService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _clientMock = new Mock<IAmazonAppConfigData>();
            _parameterStoreMock = new Mock<IParameterStoreService>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            Environment.SetEnvironmentVariable("applicationId", "App123");
            Environment.SetEnvironmentVariable("environmentId", "Env123");
            Environment.SetEnvironmentVariable("profileId", "Prof123");

            _service = new AppConfigService(_clientMock.Object, _cache, _parameterStoreMock.Object);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_CuandoExisteEnCache_RetornaValorCacheadoSinLlamarAWS()
        {
            // Arrange
            string cacheKey = "AppConfig_App123_Env123_Prof123";
            string configEsperada = "{\"featureToggle\": true}";
            _cache.Set(cacheKey, configEsperada);

            // Act
            string resultado = await _service.ObtenerConfiguracionAppConfig();

            // Assert
            Assert.AreEqual(configEsperada, resultado);
            _clientMock.Verify(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default), Times.Never);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_SinCache_ObtieneConfiguracionDesdeAWSYAlmacenaEnCache()
        {
            // Arrange
            string tokenInicial = "TokenSesion123";
            string tokenSiguiente = "TokenSesion456";
            string jsonEsperado = "{\"montoMaximo\": 5000}";

            StartConfigurationSessionResponse sessionResponse = new StartConfigurationSessionResponse
            {
                InitialConfigurationToken = tokenInicial
            };

            MemoryStream streamConfig = new MemoryStream(Encoding.UTF8.GetBytes(jsonEsperado));
            GetLatestConfigurationResponse configResponse = new GetLatestConfigurationResponse
            {
                NextPollConfigurationToken = tokenSiguiente,
                Configuration = streamConfig
            };

            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                       .ReturnsAsync(sessionResponse);

            _clientMock.Setup(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default))
                       .ReturnsAsync(configResponse);

            _parameterStoreMock.Setup(p => p.GetValueParameterStore("AppConfigCacheHours"))
                               .ReturnsAsync("2");

            // Act
            string resultado = await _service.ObtenerConfiguracionAppConfig();

            // Assert
            Assert.AreEqual(jsonEsperado, resultado);
            _clientMock.Verify(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default), Times.Once);
            _clientMock.Verify(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default), Times.Once);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_CuandoAWSFalla_RetornaFallbackSeguro()
        {
            // Arrange
            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                       .ThrowsAsync(new Exception("Error de conexión con AWS"));

            // Act
            string resultado = await _service.ObtenerConfiguracionAppConfig();

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual("{}", resultado);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_BadRequest_RenuevaTokenYReintenta()
        {
            string tokenInicial = "t1";
            string tokenRenovado = "t2";
            string json = "{\"ok\":true}";

            _clientMock.SetupSequence(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ReturnsAsync(new StartConfigurationSessionResponse { InitialConfigurationToken = tokenInicial })
                .ReturnsAsync(new StartConfigurationSessionResponse { InitialConfigurationToken = tokenRenovado });

            _clientMock.SetupSequence(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default))
                .ThrowsAsync(new BadRequestException("expired"))
                .ReturnsAsync(new GetLatestConfigurationResponse
                {
                    NextPollConfigurationToken = "t3",
                    Configuration = new MemoryStream(Encoding.UTF8.GetBytes(json))
                });

            _parameterStoreMock.Setup(p => p.GetValueParameterStore("AppConfigCacheHours")).ReturnsAsync("abc");

            string resultado = await _service.ObtenerConfiguracionAppConfig();
            Assert.AreEqual(json, resultado);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_BadRequestEnAmbosIntentos_UsaFallback()
        {
            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ReturnsAsync(new StartConfigurationSessionResponse { InitialConfigurationToken = "tok" });
            _clientMock.Setup(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default))
                .ThrowsAsync(new BadRequestException("always"));

            string resultado = await _service.ObtenerConfiguracionAppConfig();
            Assert.AreEqual("{}", resultado);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_RenewTokenFalla_UsaStoreFallbackExterno()
        {
            _clientMock.SetupSequence(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default))
                .ThrowsAsync(new BadRequestException("expired"));

            _clientMock.SetupSequence(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ReturnsAsync(new StartConfigurationSessionResponse { InitialConfigurationToken = "tok1" })
                .ThrowsAsync(new Exception("renew session down"));

            string resultado = await _service.ObtenerConfiguracionAppConfig();
            Assert.AreEqual("{}", resultado);
        }

        [TestMethod]
        public void ResolveFallback_ConCacheHit_RetornaValorCacheado()
        {
            string cacheKey = "AppConfig_App123_Env123_Prof123";
            _cache.Set(cacheKey, "{\"fromFallback\":true}");

            System.Reflection.MethodInfo method = typeof(AppConfigService)
                .GetMethod("ResolveFallback", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            string result = (string)method.Invoke(_service, new object[] { cacheKey })!;
            Assert.AreEqual("{\"fromFallback\":true}", result);
        }

        [TestMethod]
        public async Task ObtenerConfiguracionAppConfig_ConfigNull_UsaObjetoVacioYFallbackCacheado()
        {
            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ReturnsAsync(new StartConfigurationSessionResponse { InitialConfigurationToken = "tok" });
            _clientMock.Setup(c => c.GetLatestConfigurationAsync(It.IsAny<GetLatestConfigurationRequest>(), default))
                .ReturnsAsync(new GetLatestConfigurationResponse { Configuration = null });
            _parameterStoreMock.Setup(p => p.GetValueParameterStore("AppConfigCacheHours"))
                .ThrowsAsync(new Exception("ps down"));

            string first = await _service.ObtenerConfiguracionAppConfig();
            Assert.AreEqual("{}", first);

            _cache.Remove("AppConfig_App123_Env123_Prof123");
            _cache.Set("AppConfig_App123_Env123_Prof123", "{\"cached\":true}");

            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ThrowsAsync(new Exception("down"));
            // clear main path to force FetchWithRetry -> ResolveFallback with cached
            AppConfigService fresh = new(_clientMock.Object, _cache, _parameterStoreMock.Object);
            // remove so Obtener goes to fetch
            _cache.Remove("AppConfig_App123_Env123_Prof123");
            // put only as fallback after failed fetch via ResolveFallback - seed after lock failure path:
            // First call fails AWS; StoreFallback writes {}. Second scenario: cache has value for ResolveFallback.
            string fail = await fresh.ObtenerConfiguracionAppConfig();
            Assert.AreEqual("{}", fail);

            // Now with existing cache hit on ResolveFallback: fail fetch but cache already has value from StoreFallback
            _clientMock.Setup(c => c.StartConfigurationSessionAsync(It.IsAny<StartConfigurationSessionRequest>(), default))
                .ThrowsAsync(new Exception("down again"));
            string again = await fresh.ObtenerConfiguracionAppConfig();
            Assert.AreEqual("{}", again);
        }
    }
}